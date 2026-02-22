using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkFlow.MongoModels
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string Action { get; set; } = null!;
        public string PerformedBy { get; set; } = null!;
        public string Details { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
