using Microsoft.AspNetCore.Http;

namespace WorkFlow.DTOs
{
    public class DocumentUploadDto
    {
        public int UserId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
