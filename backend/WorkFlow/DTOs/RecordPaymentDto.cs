namespace WorkFlow.DTOs
{
    public class RecordPaymentDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentIntentId { get; set; } = null!;
        public string Status { get; set; } = "Paid";
    }
}
