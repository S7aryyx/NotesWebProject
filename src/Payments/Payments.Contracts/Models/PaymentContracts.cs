namespace WebPaymentTest.Models
{
    public sealed class CreatePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public sealed class CreatePaymentResponse
    {
        public string PaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Paid { get; set; }
        public string ConfirmationUrl { get; set; } = string.Empty;
    }

    public sealed class PaymentStatusResponse
    {
        public string Id { get; set; } = "";
        public string Status { get; set; } = "";
        public bool Paid { get; set; }
    }


}
