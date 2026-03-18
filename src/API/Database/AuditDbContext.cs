using API.Database.Models;
using MongoDB.Driver;

namespace API.Database
{
    public class AuditDbContext
    {
        private readonly IMongoDatabase _database;

        public AuditDbContext(IMongoClient client)
        {
            _database = client.GetDatabase("auditdb");
            EnsureIndexes();
        }

        public IMongoCollection<LogDocument> ApiLogs => _database.GetCollection<LogDocument>("api_logs");

        // Índices para queries frecuentes
        private void EnsureIndexes()
        {
            // Búsqueda por usuario
            ApiLogs.Indexes.CreateOne(new CreateIndexModel<LogDocument>(
                Builders<LogDocument>.IndexKeys.Ascending("context.user.id")));

            // Búsqueda por nivel de severidad
            ApiLogs.Indexes.CreateOne(new CreateIndexModel<LogDocument>(
                Builders<LogDocument>.IndexKeys.Ascending(l => l.Level)));

            // Búsqueda por fecha
            ApiLogs.Indexes.CreateOne(new CreateIndexModel<LogDocument>(
                Builders<LogDocument>.IndexKeys.Descending(l => l.Timestamp),
                new CreateIndexOptions
                {
                    // Descomentar para auto-eliminar logs después de 90 días
                    // ExpireAfter = TimeSpan.FromDays(90)
                }));
        }
    }
}
