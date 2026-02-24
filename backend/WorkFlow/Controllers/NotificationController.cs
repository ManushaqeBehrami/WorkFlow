using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkFlow.MongoModels;
using WorkFlow.Services;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly MongoService _mongo;

        public NotificationController(MongoService mongo)
        {
            _mongo = mongo;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _mongo.GetUserNotificationsAsync(GetUserId());
            return Ok(notifications);
        }

        [Authorize(Roles = "HR")]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _mongo.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(string id)
        {
            var changed = await _mongo.MarkNotificationReadAsync(GetUserId(), id);
            if (!changed) return NotFound();
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _mongo.MarkAllNotificationsReadAsync(GetUserId());
            return NoContent();
        }

        [Authorize(Roles = "HR")]
        [HttpPost]
        public async Task<IActionResult> Create(Notification notification)
        {
            await _mongo.AddNotificationAsync(notification);
            return Ok(notification);
        }
    }
}
