namespace WorkFlow.DTOs
{
    public class CreateLeaveDto
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = "PTO";
        public string Reason { get; set; } = "";
    }
}
