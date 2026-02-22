namespace WorkFlow.DTOs
{
    public class UpdatePaymentDto
    {
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionReference { get; set; }
    }
}
