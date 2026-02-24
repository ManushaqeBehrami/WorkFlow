namespace WorkFlow.DTOs
{
    public class SearchResultDto
    {
        public List<UserResultDto> Users { get; set; } = new();
        public List<PaymentResultDto> Payments { get; set; } = new();
        public List<LeaveResultDto> Leaves { get; set; } = new();
        public List<AuditLogResultDto> AuditLogs { get; set; } = new();
        public List<DocumentResultDto> Documents { get; set; } = new();
    }

    public class UserResultDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class PaymentResultDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
    }

    public class LeaveResultDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = "";
    }

    public class AuditLogResultDto
    {
        public string Id { get; set; } = "";
        public int UserId { get; set; }
        public string Action { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class DocumentResultDto
    {
        public string Id { get; set; } = "";
        public int UserId { get; set; }
        public string FileName { get; set; } = "";
        public string FileType { get; set; } = "";
    }
}
