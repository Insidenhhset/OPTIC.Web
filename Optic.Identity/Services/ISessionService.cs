using Optic.Identity.Models;

namespace Optic.Identity.Services
{
        public interface ISessionService
        {
            Task<Guid> CreateSessionAsync(
                int userId,
                string? ipAddress,
                string? userAgent);

            Task<bool> ValidateSessionAsync(Guid sessionId);

            Task<bool> LogoutAsync(Guid sessionId);

            Task<bool> RevokeAsync(Guid sessionId);

            Task<List<Session>> GetByUserIdAsync(int userId);
        }
    }

