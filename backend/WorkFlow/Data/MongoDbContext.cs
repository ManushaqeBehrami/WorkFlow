using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using WorkFlow.MongoModels;

namespace WorkFlow.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDb");
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("WorkFlowMongoDb");
            InitializeCollectionsAsync().GetAwaiter().GetResult();
        }

        public IMongoCollection<AuditLog> AuditLogs =>
            _database.GetCollection<AuditLog>("AuditLog");

        public IMongoCollection<Notification> Notifications =>
            _database.GetCollection<Notification>("Notification");

        public IMongoCollection<Document> Documents =>
            _database.GetCollection<Document>("Document");

        private async Task InitializeCollectionsAsync()
        {
            var collectionNames = await _database.ListCollectionNames().ToListAsync();

            if (!collectionNames.Contains("AuditLog"))
                await _database.CreateCollectionAsync("AuditLog");

            if (!collectionNames.Contains("Notification"))
                await _database.CreateCollectionAsync("Notification");

            if (!collectionNames.Contains("Document"))
                await _database.CreateCollectionAsync("Document");

            if (await AuditLogs.CountDocumentsAsync(FilterDefinition<AuditLog>.Empty) == 0)
            {
                await AuditLogs.InsertOneAsync(new AuditLog
                {
                    UserId = 0,
                    Action = "Init",
                    PerformedBy = "System",
                    Details = "Initial dummy record",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (await Notifications.CountDocumentsAsync(FilterDefinition<Notification>.Empty) == 0)
            {
                await Notifications.InsertOneAsync(new Notification
                {
                    UserId = 0,
                    Message = "Initial notification",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (await Documents.CountDocumentsAsync(FilterDefinition<Document>.Empty) == 0)
            {
                await Documents.InsertOneAsync(new Document
                {
                    UserId = 0,
                    FileName = "init.txt",
                    FileUrl = "/init.txt",
                    FileType = "txt",
                    UploadedAt = DateTime.UtcNow
                });
            }
        }
    }
}