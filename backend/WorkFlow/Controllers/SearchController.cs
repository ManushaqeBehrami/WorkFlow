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
    [Route("api/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MongoService _mongo;

        public SearchController(ApplicationDbContext context, MongoService mongo)
        {
            _context = context;
            _mongo = mongo;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "Employee";
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var term = (q ?? string.Empty).Trim();
            if (term.Length < 2)
            {
                return Ok(new SearchResultDto());
            }

            var role = GetRole();
            var userId = GetUserId();
            var result = new SearchResultDto();

            if (role == "HR")
            {
                var users = await _context.Users
                    .Where(u => EF.Functions.Like(u.FullName, $"%{term}%")
                             || EF.Functions.Like(u.Email, $"%{term}%"))
                    .Select(u => new UserResultDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = u.Role.ToString()
                    })
                    .ToListAsync();

                var payments = await _context.Payments
                    .Include(p => p.User)
                    .Where(p => EF.Functions.Like(p.User.FullName, $"%{term}%")
                             || EF.Functions.Like(p.Status, $"%{term}%"))
                    .Select(p => new PaymentResultDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserFullName = p.User.FullName,
                        Amount = p.Amount,
                        Status = p.Status
                    })
                    .ToListAsync();

                var auditLogs = await _mongo.GetAuditLogsAsync();
                var docs = await _mongo.GetAllDocumentsAsync();

                result.Users = users;
                result.Payments = payments;
                result.AuditLogs = auditLogs
                    .Where(l => l.Action.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || l.Details.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(l => new AuditLogResultDto
                    {
                        Id = l.Id,
                        UserId = l.UserId,
                        Action = l.Action,
                        Details = l.Details,
                        Timestamp = l.Timestamp
                    })
                    .ToList();
                result.Documents = docs
                    .Where(d => d.FileName.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || d.FileType.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(d => new DocumentResultDto
                    {
                        Id = d.Id,
                        UserId = d.UserId,
                        FileName = d.FileName,
                        FileType = d.FileType
                    })
                    .ToList();
            }
            else if (role == "Manager")
            {
                var users = await _context.Users
                    .Where(u => u.Role == UserRole.Employee &&
                                (EF.Functions.Like(u.FullName, $"%{term}%")
                              || EF.Functions.Like(u.Email, $"%{term}%")))
                    .Select(u => new UserResultDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = u.Role.ToString()
                    })
                    .ToListAsync();

                var leaves = await _context.LeaveRequests
                    .Where(l => EF.Functions.Like(l.Reason, $"%{term}%")
                             || EF.Functions.Like(l.Status, $"%{term}%"))
                    .Select(l => new LeaveResultDto
                    {
                        Id = l.Id,
                        UserId = l.UserId,
                        Status = l.Status,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Reason = l.Reason
                    })
                    .ToListAsync();

                result.Users = users;
                result.Leaves = leaves;
            }
            else
            {
                var payments = await _context.Payments
                    .Where(p => p.UserId == userId &&
                                (EF.Functions.Like(p.Status, $"%{term}%")))
                    .Select(p => new PaymentResultDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        UserFullName = "",
                        Amount = p.Amount,
                        Status = p.Status
                    })
                    .ToListAsync();

                var leaves = await _context.LeaveRequests
                    .Where(l => l.UserId == userId &&
                                (EF.Functions.Like(l.Reason, $"%{term}%")
                              || EF.Functions.Like(l.Status, $"%{term}%")))
                    .Select(l => new LeaveResultDto
                    {
                        Id = l.Id,
                        UserId = l.UserId,
                        Status = l.Status,
                        StartDate = l.StartDate,
                        EndDate = l.EndDate,
                        Reason = l.Reason
                    })
                    .ToListAsync();

                var docs = await _mongo.GetUserDocumentsAsync(userId);

                result.Payments = payments;
                result.Leaves = leaves;
                result.Documents = docs
                    .Where(d => d.FileName.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || d.FileType.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(d => new DocumentResultDto
                    {
                        Id = d.Id,
                        UserId = d.UserId,
                        FileName = d.FileName,
                        FileType = d.FileType
                    })
                    .ToList();
            }

            return Ok(result);
        }
    }
}
