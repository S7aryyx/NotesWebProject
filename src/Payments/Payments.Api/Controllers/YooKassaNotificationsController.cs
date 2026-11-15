
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebPaymentTest.Service;

namespace WebPaymentTest.Controllers;

[ApiController]
[Route("api/yookassa/notifications")]
public sealed class YooKassaNotificationsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<YooKassaNotificationsController> _logger;

    public YooKassaNotificationsController(
        PaymentService paymentService,
        ILogger<YooKassaNotificationsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] JsonElement notification,
        CancellationToken cancellationToken)
    {
        if (!notification.TryGetProperty("event", out var eventElement) ||
            !notification.TryGetProperty("object", out var paymentObject) ||
            !paymentObject.TryGetProperty("id", out var idElement))
        {
            return BadRequest();
        }

        string eventName = eventElement.GetString() ?? "";
        string paymentId = idElement.GetString() ?? "";

        if (string.IsNullOrWhiteSpace(paymentId))
            return BadRequest();

        _logger.LogInformation(
            "Получено событие {Event} для платежа {PaymentId}",
            eventName, paymentId);

        // Проверяем фактический статус через API ЮKassa.
        var payment = await _paymentService.GetPaymentStatusAsync(
            paymentId, cancellationToken);

        if (payment.Status == "succeeded" && payment.Paid)
        {
            _logger.LogInformation(
                "Платёж {PaymentId} успешно оплачен", paymentId);

            // Здесь впоследствии сохраним результат в БД
            // и активируем оплаченную функцию приложения.
        }
        else if (payment.Status == "canceled")
        {
            _logger.LogInformation(
                "Платёж {PaymentId} отменён", paymentId);
        }

        return Ok();
    }
}
