using API.Database.Models;

namespace API.Services.Audit
{
    public interface IAuditService
    {
        Task InsertLogAsync(LogDocument log);
        Task<List<LogDocument>> GetLogsByUserIdAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<LogDocument>> GetAllLogsAsync(int page = 1, int pageSize = 20);
    }
}
