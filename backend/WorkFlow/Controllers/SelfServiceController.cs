using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlow.Data;

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
