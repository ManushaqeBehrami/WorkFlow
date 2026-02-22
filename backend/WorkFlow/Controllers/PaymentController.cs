using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Data;
using WorkFlow.DTOs;
using WorkFlow.Models;
using WorkFlow.Services;
using WorkFlow.MongoModels;

namespace WorkFlow.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Roles = "HR")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MongoService _mongo;

        public PaymentController(ApplicationDbContext context, MongoService mongo)
        {
            _context = context;
            _mongo = mongo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] string? status)
        {
            var query = _context.Payments.AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var payments = await query.ToListAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            return Ok(payment);
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> PaySalary(int userId)
        {
            var salary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (salary == null)
                return NotFound("Salary not found");

            var payment = new Payment
            {
                UserId = userId,
                Amount = salary.BaseAmount,
                Status = "Paid",
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = userId,
                Action = "SalaryPaid",
                PerformedBy = User.Identity?.Name ?? "HR",
                Details = $"Salary paid: {salary.BaseAmount}"
            });

            await _mongo.AddNotificationAsync(new MongoModels.Notification
            {
                UserId = userId,
                Message = "Your salary has been paid."
            });

            return Ok(payment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdatePaymentDto dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            if (dto.Amount.HasValue)
                payment.Amount = dto.Amount.Value;

            if (dto.Status != null)
                payment.Status = dto.Status;

            if (dto.PaymentDate.HasValue)
                payment.PaymentDate = dto.PaymentDate.Value;

            if (dto.TransactionReference != null)
                payment.TransactionReference = dto.TransactionReference;

            await _context.SaveChangesAsync();

            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = payment.UserId,
                Action = "PaymentUpdated",
                PerformedBy = User.Identity?.Name ?? "HR",
                Details = $"Payment {id} updated"
            });

            return Ok(payment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = payment.UserId,
                Action = "PaymentDeleted",
                PerformedBy = User.Identity?.Name ?? "HR",
                Details = $"Payment {id} deleted"
            });

            return NoContent();
        }
    }
}
