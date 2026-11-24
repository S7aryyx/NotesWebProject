using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using LocalJsonModule.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LocalWebModule.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public sealed class BillingController : ControllerBase
{
    private readonly IConnectionFactory _connections;
    private readonly HttpClient _http;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        IConnectionFactory connections,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<BillingController> logger)
    {
        _connections = connections;
        _http = httpClientFactory.CreateClient("YooKassa");
        _configuration = configuration;
        _logger = logger;

        string shopId = configuration["YooKassa:ShopId"] ?? "";
        string secret = configuration["YooKassa:SecretKey"] ?? "";
        _http.BaseAddress = new Uri("https://api.yookassa.ru/v3/");
        if (!string.IsNullOrWhiteSpace(shopId) && !string.IsNullOrWhiteSpace(secret))
        {
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{secret}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreateAccessPaymentRequest request, CancellationToken ct)
    {
        if (request.Plan != "monthly" && request.Plan != "lifetime")
            return BadRequest("Укажите тариф monthly или lifetime.");

        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized();

        if (_http.DefaultRequestHeaders.Authorization is null)
            return StatusCode(503, "Платёжный провайдер не настроен. Добавьте YooKassa credentials в User Secrets.");

        decimal amount = request.Plan == "monthly" ? 349m : 4999m;
        string description = request.Plan == "monthly"
            ? "Notes — доступ к ИИ-анализу на 30 дней"
            : "Notes — единовременный доступ к ИИ-анализу";

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(ct);
        await using (var userCommand = new NpgsqlCommand("""SELECT 1 FROM "auth"."Users" WHERE id = @id;""", connection))
        {
            userCommand.Parameters.AddWithValue("id", userId);
            if (await userCommand.ExecuteScalarAsync(ct) is null)
                return NotFound("Пользователь не найден.");
        }

        string returnUrl = _configuration["YooKassa:ReturnUrl"] ?? "https://localhost:7080/payment.html?payment=return";
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var returnUri) || returnUri.Scheme != Uri.UriSchemeHttps)
            return BadRequest("YooKassa:ReturnUrl должен быть HTTPS URL.");

        var payload = new
        {
            amount = new { value = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), currency = "RUB" },
            capture = true,
            confirmation = new { type = "redirect", return_url = returnUri.ToString() },
            description,
            metadata = new { user_id = userId.ToString(), plan = request.Plan, product = "notes_ai_access" }
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "payments");
        message.Headers.Add("Idempotence-Key", Guid.NewGuid().ToString());
        message.Content = JsonContent.Create(payload);
        using var response = await _http.SendAsync(message, ct);
        string responseText = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("YooKassa payment creation failed: {Status} {Body}", response.StatusCode, responseText);
            return StatusCode(502, "Платёжный провайдер не смог создать платёж.");
        }

        using var json = JsonDocument.Parse(responseText);
        var root = json.RootElement;
        string paymentId = root.GetProperty("id").GetString() ?? "";
        string status = root.GetProperty("status").GetString() ?? "pending";
        string confirmationUrl = root.GetProperty("confirmation").GetProperty("confirmation_url").GetString() ?? "";

        await using var insert = new NpgsqlCommand("""
            INSERT INTO "billing"."PaymentTransactions"
                (id, user_id, plan, amount, currency, status, created_at)
            VALUES (@id, @user_id, @plan, @amount, 'RUB', @status, now())
            ON CONFLICT (id) DO NOTHING;
            """, connection);
        insert.Parameters.AddWithValue("id", paymentId);
        insert.Parameters.AddWithValue("user_id", userId);
        insert.Parameters.AddWithValue("plan", request.Plan);
        insert.Parameters.AddWithValue("amount", amount);
        insert.Parameters.AddWithValue("status", status);
        await insert.ExecuteNonQueryAsync(ct);

        return Ok(new { paymentId, status, confirmationUrl });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized();

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand("""
            SELECT has_lifetime_access, subscription_until
            FROM "billing"."UserAccess"
            WHERE user_id = @user_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        await using var reader = await command.ExecuteReaderAsync(ct);

        bool lifetime = false;
        DateTime? until = null;
        if (await reader.ReadAsync(ct))
        {
            lifetime = reader.GetBoolean(0);
            until = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
        }

        bool subscriptionActive = until.HasValue && until.Value > DateTime.UtcNow;
        return Ok(new
        {
            hasAiAccess = lifetime || subscriptionActive,
            plan = lifetime ? "lifetime" : subscriptionActive ? "monthly" : "none",
            hasLifetimeAccess = lifetime,
            subscriptionUntil = until
        });
    }

    [HttpGet("ai-token")]
    public async Task<IActionResult> GetAiToken(CancellationToken ct)
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized();

        string signingKey = _configuration["AiAccess:SigningKey"] ?? "";
        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            return StatusCode(503, "Не настроен AiAccess:SigningKey.");

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand("""
            SELECT has_lifetime_access, subscription_until
            FROM "billing"."UserAccess"
            WHERE user_id = @user_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        await using var reader = await command.ExecuteReaderAsync(ct);

        bool lifetime = false;
        DateTime? until = null;
        if (await reader.ReadAsync(ct))
        {
            lifetime = reader.GetBoolean(0);
            until = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
        }

        if (!lifetime && (!until.HasValue || until.Value <= DateTime.UtcNow))
            return Forbid();

        long expiresAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        string payload = $"{userId}:{expiresAt}";
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        string payloadPart = Convert.ToBase64String(payloadBytes);
        byte[] signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), payloadBytes);
        string token = payloadPart + "." + Convert.ToBase64String(signature);

        return Ok(new { token, expiresAt });
    }

    [AllowAnonymous]
    [HttpPost("notifications/yookassa")]
    public async Task<IActionResult> YooKassaNotification([FromBody] JsonElement notification, CancellationToken ct)
    {
        if (!notification.TryGetProperty("object", out var paymentObject) ||
            !paymentObject.TryGetProperty("id", out var idElement))
            return BadRequest();

        if (_http.DefaultRequestHeaders.Authorization is null)
            return StatusCode(503);

        string paymentId = idElement.GetString() ?? "";
        if (string.IsNullOrWhiteSpace(paymentId))
            return BadRequest();

        using var response = await _http.GetAsync($"payments/{Uri.EscapeDataString(paymentId)}", ct);
        if (!response.IsSuccessStatusCode)
            return StatusCode(502);

        using var paymentJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = paymentJson.RootElement;
        string status = root.GetProperty("status").GetString() ?? "";
        bool paid = root.TryGetProperty("paid", out var paidElement) && paidElement.GetBoolean();

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);

        Guid userId;
        string plan;
        decimal expectedAmount;
        string expectedCurrency;
        bool wasAlreadyPaid;

        // Lock the transaction row so duplicate YooKassa notifications are idempotent.
        await using (var findPayment = new NpgsqlCommand("""
            SELECT user_id, plan, amount, currency, paid
            FROM "billing"."PaymentTransactions"
            WHERE id = @id
            FOR UPDATE;
            """, connection, tx))
        {
            findPayment.Parameters.AddWithValue("id", paymentId);
            await using var reader = await findPayment.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                await reader.CloseAsync();
                await tx.RollbackAsync(ct);
                // Payment row may be committed shortly after provider creation; ask YooKassa to retry.
                return StatusCode(503);
            }

            userId = reader.GetGuid(0);
            plan = reader.GetString(1);
            expectedAmount = reader.GetDecimal(2);
            expectedCurrency = reader.GetString(3).Trim();
            wasAlreadyPaid = reader.GetBoolean(4);
        }

        decimal actualAmount = 0m;
        string actualCurrency = "";
        if (root.TryGetProperty("amount", out var amountElement))
        {
            decimal.TryParse(
                amountElement.GetProperty("value").GetString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out actualAmount);
            actualCurrency = amountElement.GetProperty("currency").GetString() ?? "";
        }

        if (actualAmount != expectedAmount || actualCurrency != expectedCurrency)
        {
            await tx.RollbackAsync(ct);
            _logger.LogWarning("Payment amount/currency mismatch for {PaymentId}", paymentId);
            return BadRequest();
        }

        await using (var updatePayment = new NpgsqlCommand("""
            UPDATE "billing"."PaymentTransactions"
            SET status = @status, paid = @paid, updated_at = now()
            WHERE id = @id;
            """, connection, tx))
        {
            updatePayment.Parameters.AddWithValue("status", status);
            updatePayment.Parameters.AddWithValue("paid", paid);
            updatePayment.Parameters.AddWithValue("id", paymentId);
            await updatePayment.ExecuteNonQueryAsync(ct);
        }

        // Do not grant access twice when YooKassa retries the same succeeded event.
        if (status == "succeeded" && paid && !wasAlreadyPaid)
        {
            if (plan == "lifetime")
            {
                await using var grant = new NpgsqlCommand("""
                    INSERT INTO "billing"."UserAccess"
                        (user_id, has_lifetime_access, subscription_until, updated_at)
                    VALUES (@user_id, TRUE, NULL, now())
                    ON CONFLICT (user_id) DO UPDATE SET
                        has_lifetime_access = TRUE,
                        subscription_until = NULL,
                        updated_at = now();
                    """, connection, tx);
                grant.Parameters.AddWithValue("user_id", userId);
                await grant.ExecuteNonQueryAsync(ct);
            }
            else if (plan == "monthly")
            {
                await using var grant = new NpgsqlCommand("""
                    INSERT INTO "billing"."UserAccess"
                        (user_id, has_lifetime_access, subscription_until, updated_at)
                    VALUES (@user_id, FALSE, now() + interval '30 days', now())
                    ON CONFLICT (user_id) DO UPDATE SET
                        subscription_until = GREATEST(
                            COALESCE("UserAccess".subscription_until, now()),
                            now()) + interval '30 days',
                        updated_at = now()
                    WHERE NOT "UserAccess".has_lifetime_access;
                    """, connection, tx);
                grant.Parameters.AddWithValue("user_id", userId);
                await grant.ExecuteNonQueryAsync(ct);
            }
        }
        await tx.CommitAsync(ct);
        return Ok();
    }

    public sealed class CreateAccessPaymentRequest
    {
        public string Plan { get; set; } = "";
    }
}
