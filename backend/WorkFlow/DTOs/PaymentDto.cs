namespace WorkFlow.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; }
        public string? TransactionReference { get; set; }
    }
}
