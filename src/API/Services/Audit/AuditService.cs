using API.Database;
using API.Database.Models;
using MongoDB.Driver;

namespace API.Services.Audit
{
    public class AuditService(AuditDbContext _logsContext) : IAuditService
    {
        public Task InsertLogAsync(LogDocument log) => _logsContext.ApiLogs.InsertOneAsync(log);

        public async Task<List<LogDocument>> GetLogsByUserIdAsync(
            string userId, int page = 1, int pageSize = 20) =>
            await _logsContext.ApiLogs
                .Find(l => l.Context.User.Id == userId)
                .SortByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

        public async Task<List<LogDocument>> GetAllLogsAsync(
            int page = 1, int pageSize = 20) =>
            await _logsContext.ApiLogs
                .Find(_ => true)
                .SortByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
    }
}
