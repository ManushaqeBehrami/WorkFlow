using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlow.Data;
using WorkFlow.DTOs;
using WorkFlow.Models;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/leaves")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
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

            return Ok(leave);
        }
    }
}
