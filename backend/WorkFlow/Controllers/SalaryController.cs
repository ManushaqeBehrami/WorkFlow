using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Data;
using WorkFlow.DTOs;
using WorkFlow.Models;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/salaries")]
    [Authorize(Roles = "HR")]
    public class SalaryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalaryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId)
        {
            var query = _context.Salaries.AsQueryable();

            if (userId.HasValue)
                query = query.Where(s => s.UserId == userId.Value);

            var salaries = await query.ToListAsync();
            return Ok(salaries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null) return NotFound();
            return Ok(salary);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSalaryDto dto)
        {
            var salary = new Salary
            {
                UserId = dto.UserId,
                BaseAmount = dto.BaseAmount
            };

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return Ok(salary);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSalaryDto dto)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null) return NotFound();

            if (dto.UserId.HasValue) salary.UserId = dto.UserId.Value;
            if (dto.BaseAmount.HasValue) salary.BaseAmount = dto.BaseAmount.Value;
            if (dto.Currency != null) salary.Currency = dto.Currency;
            if (dto.EffectiveFrom.HasValue) salary.EffectiveFrom = dto.EffectiveFrom.Value;

            await _context.SaveChangesAsync();
            return Ok(salary);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null) return NotFound();

            _context.Salaries.Remove(salary);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
