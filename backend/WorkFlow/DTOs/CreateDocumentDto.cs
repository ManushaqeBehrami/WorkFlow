namespace WorkFlow.DTOs
{
    public class CreateDocumentDto
    {
        public int UserId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string FileType { get; set; } = null!;
    }
}
