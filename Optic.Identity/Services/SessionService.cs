
using Microsoft.Extensions.Options;
using Optic.Identity.Models;
using Optic.Identity.Repositories;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly SessionSettings _settings;

        public SessionService(
            ISessionRepository sessionRepository,
            IOptions<SessionSettings> settings)
        {
            _sessionRepository = sessionRepository;
            _settings = settings.Value;
        }

        public async Task<Guid> CreateSessionAsync(
            int userId,
            string? ipAddress,
            string? userAgent)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            var now = DateTime.UtcNow;
            var sessionId = Guid.NewGuid();

            var session = new Session
            {
                SessionId = sessionId,
                UserId = userId,
                LoginAt = now,
                LastActivityAt = now,
                ExpiresAt = now.AddMinutes(_settings.AbsoluteTimeoutMinutes),
                IsRevoked = false,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            return await _sessionRepository.CreateAsync(session);
        }

        public Task<bool> ValidateSessionAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                return Task.FromResult(false);

            return _sessionRepository.ValidateAndUpdateActivityAsync(
                sessionId,
                _settings.IdleTimeoutMinutes,
                _settings.ActivityUpdateIntervalSeconds);
        }

        public Task<bool> LogoutAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                return Task.FromResult(false);

            return _sessionRepository.LogoutAsync(sessionId);
        }

        public Task<bool> RevokeAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                return Task.FromResult(false);

            return _sessionRepository.RevokeAsync(sessionId);
        }

        public Task<List<Session>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            return _sessionRepository.GetByUserIdAsync(userId);
        }
    }
}
