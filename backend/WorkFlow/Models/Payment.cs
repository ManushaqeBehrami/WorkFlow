namespace WorkFlow.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = "Pending";
        // Pending, Paid, Failed

        public DateTime PaymentDate { get; set; }

        public string? TransactionReference { get; set; }

        public User User { get; set; } = null!;
    }
}
