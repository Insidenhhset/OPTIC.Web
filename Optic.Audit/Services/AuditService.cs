
using Optic.Audit.Models;

namespace Optic.Audit.Services
{
    public class AuditService : IAuditService
    {
        private static readonly List<AuditLog> AuditLogs = [];

        private static readonly object LockObject = new();

        private long _nextId;

        public Task<long> LogActivityAsync(AuditLog auditLog)
        {
            lock (LockObject)
            {
                auditLog.Id = ++_nextId;

                auditLog.CreatedAt = DateTime.UtcNow;

                AuditLogs.Add(auditLog);
            }

            return Task.FromResult(auditLog.Id);
        }

        public Task<AuditLog?> GetAuditByIdAsync(long id)
        {
            lock (LockObject)
            {
                var auditLog = AuditLogs.FirstOrDefault(x => x.Id == id);
                return Task.FromResult(auditLog);
            }
        }

        public Task<PagedResult<AuditLog>> GetAuditLogsAsync(
            AuditLogFilter filter)
        {
            lock (LockObject)
            {
                var query = AuditLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.UserId))
                    query = query.Where(x => x.UserId == filter.UserId);

                if (!string.IsNullOrWhiteSpace(filter.ModuleName))
                    query = query.Where(x =>
                        x.ModuleName == filter.ModuleName);

                if (!string.IsNullOrWhiteSpace(filter.Action))
                    query = query.Where(x => x.Action == filter.Action);

                if (!string.IsNullOrWhiteSpace(filter.EntityName))
                    query = query.Where(x =>
                        x.EntityName == filter.EntityName);

                if (filter.FromDate.HasValue)
                    query = query.Where(x =>
                        x.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(x =>
                        x.CreatedAt <= filter.ToDate.Value);

                var totalRecords = query.Count();

                var pageNumber = Math.Max(1, filter.PageNumber);

                var pageSize = Math.Clamp(filter.PageSize, 1, 100);

                var items = query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PagedResult<AuditLog>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                });
            }
        }

        public Task<IReadOnlyList<AuditLog>> GetRecentActivitiesAsync(
            int count = 10)
        {
            lock (LockObject)
            {
                var activities = AuditLogs
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(Math.Clamp(count, 1, 100))
                    .ToList();

                return Task.FromResult<IReadOnlyList<AuditLog>>(
                    activities);
            }
        }
    }
}
