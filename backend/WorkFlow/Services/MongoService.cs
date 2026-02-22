using MongoDB.Driver;
using WorkFlow.Data;
using WorkFlow.MongoModels;

namespace WorkFlow.Services
{
    public class MongoService
    {
        private readonly MongoDbContext _context;
        public MongoService(MongoDbContext context)
        {
            _context = context;
        }
        public async Task AddLogAsync(AuditLog log)
        {
            await _context.AuditLogs.InsertOneAsync(log);
        }
        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.InsertOneAsync(notification);
        }
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Find(n => n.UserId == userId)
                .ToListAsync();
        }
        public async Task AddDocumentAsync(Document document)
        {
            await _context.Documents.InsertOneAsync(document);
        }
        public async Task<List<Document>> GetUserDocumentsAsync(int userId)
        {
            return await _context.Documents
                .Find(d => d.UserId == userId)
                .ToListAsync();
        }
    }
}
