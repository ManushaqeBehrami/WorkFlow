namespace WorkFlow.DTOs
{
    public class UpdateSalaryDto
    {
        public int? UserId { get; set; }
        public decimal? BaseAmount { get; set; }
        public string? Currency { get; set; }
        public DateTime? EffectiveFrom { get; set; }
    }
}
