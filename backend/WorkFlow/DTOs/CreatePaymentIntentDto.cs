namespace WorkFlow.DTOs
{
    public class CreatePaymentIntentDto
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
