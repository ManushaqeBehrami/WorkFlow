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
        public async Task<List<AuditLog>> GetAuditLogsAsync(int? userId = null)
        {
            var query = userId.HasValue
                ? _context.AuditLogs.Find(l => l.UserId == userId.Value)
                : _context.AuditLogs.Find(_ => true);
            return await query.ToListAsync();
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
        public async Task<Document?> GetDocumentByIdAsync(string id)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                return null;

            var filter = MongoDB.Driver.Builders<Document>.Filter.Eq("_id", objectId);
            return await _context.Documents
                .Find(filter)
                .FirstOrDefaultAsync();
        }
        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents.Find(_ => true).ToListAsync();
        }
    }
}
