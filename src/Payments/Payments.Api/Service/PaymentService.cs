using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using WebPaymentTest.Models;

namespace WebPaymentTest.Service;

public class PaymentService
{
    private readonly HttpClient _httpClient;
    public PaymentService(HttpClient httpClient,IConfiguration configuration)
    {
        _httpClient = httpClient;

        string shopId = configuration["YooKassa:ShopId"] ?? throw new Exception("ID магазина не указан.");
        string secretKey = configuration["YooKassa:SecretKey"] ?? throw new Exception("Ключ к магазину не указан.");

        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{secretKey}"));

        _httpClient.BaseAddress = new Uri("https://api.yookassa.ru/v3/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
    public async Task<CreatePaymentResponse> CreatePaymentAsync(CreatePaymentRequest dto, string returnUrl, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Сумма должна быть больше нуля.");
        }
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("ReturnUrl должен быть HTTPS-адресом.");
        }
        string idempotenceKey = Guid.NewGuid().ToString();

        var PaymentData = new
        {
            amount = new
            {
                value = dto.Amount.ToString(),
                currency = "RUB"
            },
            capture = true,
            confirmation = new
            {
                type = "redirect",
                return_url = uri.ToString()
            },
            description = dto.Description,
            metadata = new
            {
                source = "WebPaymentTest"
            }
        };

        //отправляем запрос на кассу для заполнения данных + оплаты.

        using var request = new HttpRequestMessage(HttpMethod.Post, "payments");
        request.Headers.Add("Idempotence-Key", idempotenceKey);
        request.Content = JsonContent.Create(PaymentData);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        string text = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Ю-касса вернула: {response.StatusCode}:\n{text}");
        }

        using var document = JsonDocument.Parse(text);

        string payment_id = document.RootElement.GetProperty("id").ToString();
        string status = document.RootElement.GetProperty("status").ToString();
        bool paid = document.RootElement.GetProperty("paid").GetBoolean();
        string confirmationUrl = document.RootElement.GetProperty("confirmation").GetProperty("confirmation_url").ToString();

        return new CreatePaymentResponse
        {
            PaymentId = payment_id,
            Status = status,
            Paid = paid,
            ConfirmationUrl = confirmationUrl
        };
        
        
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string paymentId,CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"payments/{Uri.EscapeDataString(paymentId)}",cancellationToken);

        response.EnsureSuccessStatusCode();
        var payment = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>(cancellationToken: cancellationToken);
        return payment ?? throw new InvalidOperationException("ЮKassa вернула пустой ответ.");
    }


}
