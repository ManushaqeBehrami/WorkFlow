using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Data;
using WorkFlow.Models;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "HR")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL USERS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET USER BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // UPDATE USER ROLE
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, UserRole role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            if (role != UserRole.Employee)
                user.ManagerId = null;
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // ASSIGN/UNASSIGN EMPLOYEE TO MANAGER
        [HttpPut("{id}/manager")]
        public async Task<IActionResult> UpdateManager(int id, [FromQuery] int? managerId)
        {
            var employee = await _context.Users.FindAsync(id);
            if (employee == null) return NotFound();
            if (employee.Role != UserRole.Employee)
                return BadRequest("Only employees can be assigned to a manager.");

            if (managerId.HasValue)
            {
                if (managerId.Value == id)
                    return BadRequest("Employee cannot be their own manager.");

                var manager = await _context.Users.FindAsync(managerId.Value);
                if (manager == null || manager.Role != UserRole.Manager)
                    return BadRequest("Selected manager is invalid.");
            }

            employee.ManagerId = managerId;
            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        // DELETE USER
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
