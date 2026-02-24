using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlow.Data;
using WorkFlow.Models;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class SelfServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SelfServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // MY SALARY
        [HttpGet("salary")]
        public async Task<IActionResult> GetMySalary()
        {
            var userId = GetUserId();
            var salary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return Ok(salary);
        }

        // MY PAYMENTS
        [HttpGet("payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetUserId();
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return Ok(payments);
        }

        // MANAGER: MY TEAM
        [Authorize(Roles = "Manager")]
        [HttpGet("team")]
        public async Task<IActionResult> GetMyTeam()
        {
            var userId = GetUserId();
            var team = await _context.Users
                .Where(u => u.Role == UserRole.Employee && u.ManagerId == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.ManagerId
                })
                .ToListAsync();

            return Ok(team);
        }

        // MY LEAVES
        [HttpGet("leaves")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userId = GetUserId();
            var leaves = await _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return Ok(leaves);
        }
    }
}
