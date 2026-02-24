using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFlow.Data;
using WorkFlow.DTOs;
using WorkFlow.Models;
using WorkFlow.Services;
using WorkFlow.MongoModels;
using Stripe;

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
            var query = _context.Payments.Include(p => p.User).AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserFullName = p.User.FullName,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    TransactionReference = p.TransactionReference
                })
                .ToListAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound();

            return Ok(new PaymentDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                UserFullName = payment.User.FullName,
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionReference = payment.TransactionReference
            });
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

        [HttpPost("intent")]
        public async Task<IActionResult> CreatePaymentIntent(CreatePaymentIntentDto dto, [FromServices] IConfiguration config)
        {
            if (dto.Amount <= 0) return BadRequest("Amount must be greater than 0.");

            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
                return BadRequest("Stripe secret key is not configured.");

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100),
                Currency = dto.Currency,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", dto.UserId.ToString() }
                }
            });

            return Ok(new { clientSecret = intent.ClientSecret, paymentIntentId = intent.Id });
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordPayment(RecordPaymentDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("User not found");

            var payment = new Payment
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                Status = dto.Status,
                PaymentDate = DateTime.UtcNow,
                TransactionReference = dto.PaymentIntentId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = dto.UserId,
                Action = "PaymentRecorded",
                PerformedBy = User.Identity?.Name ?? "HR",
                Details = $"Payment recorded: {dto.Amount}"
            });

            await _mongo.AddNotificationAsync(new MongoModels.Notification
            {
                UserId = dto.UserId,
                Message = "A payment has been processed."
            });

            return Ok(new PaymentDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                UserFullName = user.FullName,
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionReference = payment.TransactionReference
            });
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

            var user = await _context.Users.FindAsync(payment.UserId);
            return Ok(new PaymentDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                UserFullName = user?.FullName ?? "",
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionReference = payment.TransactionReference
            });
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
