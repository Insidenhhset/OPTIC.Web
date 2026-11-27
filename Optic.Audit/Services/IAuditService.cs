
using Optic.Audit.Models;

namespace Optic.Audit.Services
{
    public interface IAuditService
    {
        Task<long> LogActivityAsync(AuditLog auditLog);
        Task<AuditLog?> GetAuditByIdAsync(long id);
        Task<PagedResult<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
        Task<IReadOnlyList<AuditLog>> GetRecentActivitiesAsync(int count = 10);
    }
}
