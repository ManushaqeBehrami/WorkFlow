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

        // EMPLOYEE: CREATE REQUEST
        [HttpPost]
        public async Task<IActionResult> Create(CreateLeaveDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var leave = new LeaveRequest
            {
                UserId = userId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
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
            var leaves = await _context.LeaveRequests.ToListAsync();
            return Ok(leaves);
        }

        // MANAGER: APPROVE / REJECT
        [Authorize(Roles = "Manager")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            leave.Status = status;
            await _context.SaveChangesAsync();

            return Ok(leave);
        }
    }
}
