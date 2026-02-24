using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkFlow.Services;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Roles = "HR")]
    public class AuditLogsController : ControllerBase
    {
        private readonly MongoService _mongo;

        public AuditLogsController(MongoService mongo)
        {
            _mongo = mongo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            var logs = await _mongo.GetAuditLogsAsync(userId);
            return Ok(logs.OrderByDescending(l => l.Timestamp));
        }
    }
}
