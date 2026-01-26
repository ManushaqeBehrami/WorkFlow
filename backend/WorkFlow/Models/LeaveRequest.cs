namespace WorkFlow.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Type { get; set; } = null!;
        // PTO, Sick, Emergency

        public string Status { get; set; } = "Pending";
        // Pending, Approved, Rejected

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
