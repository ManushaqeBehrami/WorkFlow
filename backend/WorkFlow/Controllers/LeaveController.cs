using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlow.Data;
using WorkFlow.DTOs;
using WorkFlow.Models;
using WorkFlow.Services;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/leaves")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MongoService _mongo;

        public LeaveController(ApplicationDbContext context, MongoService mongo)
        {
            _context = context;
            _mongo = mongo;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // EMPLOYEE: CREATE REQUEST
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            if (dto.StartDate > dto.EndDate)
                return BadRequest(new { message = "StartDate cannot be after EndDate." });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var leave = new LeaveRequest
            {
                UserId = userId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Type = string.IsNullOrWhiteSpace(dto.Type) ? "PTO" : dto.Type.Trim(),
                Reason = dto.Reason
            };

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();

            var employee = await _context.Users.FindAsync(userId);
            if (employee != null)
            {
                await _mongo.AddNotificationAsync(new WorkFlow.MongoModels.Notification
                {
                    UserId = userId,
                    Message = $"Your PTO request ({dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}) was submitted."
                });

                if (employee.ManagerId.HasValue)
                {
                    await _mongo.AddNotificationAsync(new WorkFlow.MongoModels.Notification
                    {
                        UserId = employee.ManagerId.Value,
                        Message = $"{employee.FullName} submitted a PTO request ({dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd})."
                    });
                }
            }

            return Ok(leave);
        }

        // HR: VIEW ALL REQUESTS
        [Authorize(Roles = "HR,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.LeaveRequests
                .Include(l => l.User)
                .AsQueryable();

            if (User.IsInRole("Manager"))
            {
                var managerId = GetUserId();
                query = query.Where(l => l.User.ManagerId == managerId);
            }

            var leaves = await query
                .OrderByDescending(l => l.StartDate)
                .Select(l => new
                {
                    l.Id,
                    l.UserId,
                    UserFullName = l.User.FullName,
                    l.StartDate,
                    l.EndDate,
                    l.Status,
                    l.Type,
                    l.Reason,
                    l.CreatedAt
                })
                .ToListAsync();

            return Ok(leaves);
        }

        // MANAGER: APPROVE / REJECT
        [Authorize(Roles = "Manager")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var managerId = GetUserId();
            var leave = await _context.LeaveRequests
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return NotFound();
            if (leave.User.ManagerId != managerId) return Forbid();

            leave.Status = status;
            await _context.SaveChangesAsync();

            await _mongo.AddNotificationAsync(new WorkFlow.MongoModels.Notification
            {
                UserId = leave.UserId,
                Message = $"Your PTO request for {leave.StartDate:yyyy-MM-dd} to {leave.EndDate:yyyy-MM-dd} was {status}."
            });

            return Ok(leave);
        }
    }
}
