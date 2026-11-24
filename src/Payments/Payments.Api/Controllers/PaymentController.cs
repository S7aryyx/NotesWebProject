using WebPaymentTest.Service;
using WebPaymentTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebPaymentTest.Controllers
{
    [ApiController]
    [Route("api/payment")]

    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentsController(PaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request,
            CancellationToken token)
        {
            try
            {
                string returnURL = _configuration["YooKassa:ReturnUrl"]
                    ?? throw new Exception("Возвратная ссылка не указана.");

                var result = await _paymentService.CreatePaymentAsync(request, returnURL, token);

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new
                {
                    error = "payment_service_error",
                    message = "Не удалось вызвать / обратитьсмя к платёжному сервису"
                });
            }
            catch (ArgumentException ex)
            {
                return StatusCode(501, new
                {
                    error = "invalid_operation",
                    message = ex.Message
                });
            }
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentStatus(
            string paymentId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return BadRequest(new
                {
                    error = "invalid_payment_id"
                });
            }

            try
            {
                var payment = await _paymentService.GetPaymentStatusAsync(
                    paymentId,
                    cancellationToken);

                return Ok(payment);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    error = "payment_provider_error",
                    message = "Не удалось получить статус платежа от ЮKassa."
                });
            }
        }

    }

}
