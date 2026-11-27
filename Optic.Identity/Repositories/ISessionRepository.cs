using Optic.Identity.Models;

namespace Optic.Identity.Repositories
{
    public interface ISessionRepository
    {
        Task<Guid> CreateAsync(Session session);

        Task<bool> ValidateAndUpdateActivityAsync(
            Guid sessionId,
            int idleTimeoutMinutes,
            int activityUpdateIntervalSeconds);

        Task<bool> LogoutAsync(Guid sessionId);

        Task<bool> RevokeAsync(Guid sessionId);

        Task<List<Session>> GetByUserIdAsync(int userId);
    }
}