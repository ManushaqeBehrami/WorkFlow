using WorkFlow.Data;
using WorkFlow.Models;
using WorkFlow.MongoModels;

namespace WorkFlow.Services
{
    public class SeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly MongoService _mongo;
        private readonly IPasswordService _passwords;
        private readonly LocalContractFileService _localContractFiles;

        public SeedService(
            ApplicationDbContext context,
            MongoService mongo,
            IPasswordService passwords,
            LocalContractFileService localContractFiles)
        {
            _context = context;
            _mongo = mongo;
            _passwords = passwords;
            _localContractFiles = localContractFiles;
        }

        public async Task<bool> SeedAsync()
        {
            if (_context.Users.Any())
                return false;

            var hr = new User
            {
                FullName = "Avery James",
                Email = "hr@workflow.local",
                Role = UserRole.HR,
                PasswordHash = _passwords.Hash("Password123!")
            };
            var manager = new User
            {
                FullName = "Morgan Lee",
                Email = "manager@workflow.local",
                Role = UserRole.Manager,
                PasswordHash = _passwords.Hash("Password123!")
            };
            var employee1 = new User
            {
                FullName = "Maya Johnson",
                Email = "maya.johnson@workflow.local",
                Role = UserRole.Employee,
                PasswordHash = _passwords.Hash("Password123!")
            };
            var employee2 = new User
            {
                FullName = "Carlos Ruiz",
                Email = "carlos.ruiz@workflow.local",
                Role = UserRole.Employee,
                PasswordHash = _passwords.Hash("Password123!")
            };

            _context.Users.AddRange(hr, manager, employee1, employee2);
            await _context.SaveChangesAsync();

            employee1.ManagerId = manager.Id;
            employee2.ManagerId = manager.Id;
            await _context.SaveChangesAsync();

            _context.Salaries.AddRange(
                new Salary { UserId = employee1.Id, BaseAmount = 3200 },
                new Salary { UserId = employee2.Id, BaseAmount = 2800 }
            );

            _context.Payments.AddRange(
                new Payment { UserId = employee1.Id, Amount = 3200, Status = "Paid", PaymentDate = DateTime.UtcNow.AddDays(-3) },
                new Payment { UserId = employee2.Id, Amount = 2800, Status = "Paid", PaymentDate = DateTime.UtcNow.AddDays(-3) }
            );

            _context.LeaveRequests.AddRange(
                new LeaveRequest
                {
                    UserId = employee1.Id,
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(9),
                    Type = "PTO",
                    Reason = "Family event",
                    Status = "Pending"
                },
                new LeaveRequest
                {
                    UserId = employee2.Id,
                    StartDate = DateTime.UtcNow.AddDays(14),
                    EndDate = DateTime.UtcNow.AddDays(16),
                    Type = "PTO",
                    Reason = "Medical",
                    Status = "Approved"
                }
            );

            await _context.SaveChangesAsync();

            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = employee1.Id,
                Action = "Seeded",
                PerformedBy = "System",
                Details = "Seed data created for employee 1"
            });
            await _mongo.AddLogAsync(new AuditLog
            {
                UserId = employee2.Id,
                Action = "Seeded",
                PerformedBy = "System",
                Details = "Seed data created for employee 2"
            });

            await _mongo.AddNotificationAsync(new WorkFlow.MongoModels.Notification
            {
                UserId = employee1.Id,
                Message = "Welcome to WorkFlow. Your profile has been created."
            });

            await _mongo.AddDocumentAsync(new Document
            {
                UserId = employee1.Id,
                FileName = "Employment_Contract_2026.pdf",
                FileUrl = _localContractFiles.EnsureSeedContractFile(),
                FileType = "application/pdf"
            });

            return true;
        }
    }
}
