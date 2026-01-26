namespace WorkFlow.Models
{
    public class Salary
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal BaseAmount { get; set; }

        public string Currency { get; set; } = "EUR";

        public DateTime EffectiveFrom { get; set; }

        public User User { get; set; } = null!;
    }
}
