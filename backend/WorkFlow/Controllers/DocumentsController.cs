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

        public DocumentsController(MongoService mongo)
        {
            _mongo = mongo;
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
            return Ok(docs);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var docs = await _mongo.GetUserDocumentsAsync(userId);
            return Ok(docs);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserId();
            var docs = await _mongo.GetUserDocumentsAsync(userId);
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
    }
}
