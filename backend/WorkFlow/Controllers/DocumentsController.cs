using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkFlow.DTOs;
using WorkFlow.MongoModels;
using WorkFlow.Services;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly MongoService _mongo;
        private readonly IWebHostEnvironment _env;
        private readonly LocalContractFileService _localContractFiles;

        public DocumentsController(MongoService mongo, IWebHostEnvironment env, LocalContractFileService localContractFiles)
        {
            _mongo = mongo;
            _env = env;
            _localContractFiles = localContractFiles;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [Authorize(Roles = "HR")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _mongo.GetAllDocumentsAsync();
            docs.ForEach(NormalizeDocumentUrl);
            return Ok(docs);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var docs = await _mongo.GetUserDocumentsAsync(userId);
            docs.ForEach(NormalizeDocumentUrl);
            return Ok(docs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var doc = await _mongo.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            var userId = GetUserId();
            var isHr = User.IsInRole("HR");
            if (!isHr && doc.UserId != userId)
                return Forbid();

            NormalizeDocumentUrl(doc);
            return Ok(doc);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserId();
            var docs = await _mongo.GetUserDocumentsAsync(userId);
            docs.ForEach(NormalizeDocumentUrl);
            return Ok(docs);
        }

        [Authorize(Roles = "HR")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateDocumentDto dto)
        {
            var document = new Document
            {
                UserId = dto.UserId,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl,
                FileType = dto.FileType
            };

            await _mongo.AddDocumentAsync(document);
            return Ok(document);
        }

        [Authorize(Roles = "HR")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
        {
            var file = dto.File;
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");
            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");
            if (!string.IsNullOrEmpty(file.ContentType) && !file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsRoot, safeFileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/{safeFileName}";

            var document = new Document
            {
                UserId = dto.UserId,
                FileName = file.FileName,
                FileType = file.ContentType ?? "application/octet-stream",
                FileUrl = fileUrl
            };

            await _mongo.AddDocumentAsync(document);
            await _mongo.AddNotificationAsync(new Notification
            {
                UserId = dto.UserId,
                Message = $"A new contract document \"{file.FileName}\" has been uploaded for you."
            });
            return Ok(document);
        }

        private void NormalizeDocumentUrl(Document doc)
        {
            if (string.IsNullOrWhiteSpace(doc.FileUrl))
                return;

            if (Uri.TryCreate(doc.FileUrl, UriKind.Absolute, out var absoluteUri))
            {
                if (string.Equals(absoluteUri.Host, "example.com", StringComparison.OrdinalIgnoreCase))
                {
                    doc.FileUrl = ToAbsoluteUrl(_localContractFiles.EnsureSeedContractFile());
                    doc.FileType = "application/pdf";
                    return;
                }

                return;
            }

            doc.FileUrl = ToAbsoluteUrl(doc.FileUrl);
        }

        private string ToAbsoluteUrl(string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal))
                return $"{Request.Scheme}://{Request.Host}{path}";

            return $"{Request.Scheme}://{Request.Host}/{path}";
        }
    }
}
