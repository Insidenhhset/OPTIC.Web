using System.Data;
using Microsoft.Data.SqlClient;
using Optic.Identity.Data;
using Optic.Identity.Models;

namespace Optic.Identity.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SessionRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Session session)
        {
            await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_UserSessions_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = session.SessionId;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = session.UserId;
            command.Parameters.Add("@LoginAt", SqlDbType.DateTime2).Value = session.LoginAt;
            command.Parameters.Add("@LastActivityAt", SqlDbType.DateTime2).Value = session.LastActivityAt;
            command.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = session.ExpiresAt;
            command.Parameters.Add("@IpAddress", SqlDbType.VarChar, 50).Value =
                (object?)session.IpAddress ?? DBNull.Value;
            command.Parameters.Add("@UserAgent", SqlDbType.VarChar, 500).Value =
                (object?)session.UserAgent ?? DBNull.Value;

            var result = await command.ExecuteScalarAsync();

            return result is Guid sessionId
                ? sessionId
                : throw new InvalidOperationException("Session creation failed.");
        }

        public async Task<bool> ValidateAndUpdateActivityAsync(
            Guid sessionId,
            int idleTimeoutMinutes,
            int activityUpdateIntervalSeconds)
        {
            await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_UserSessions_ValidateAndUpdateActivity", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = sessionId;
            command.Parameters.Add("@IdleTimeoutMinutes", SqlDbType.Int).Value = idleTimeoutMinutes;
            command.Parameters.Add("@ActivityUpdateIntervalSeconds", SqlDbType.Int).Value =
                activityUpdateIntervalSeconds;

            var result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToBoolean(result);
        }

        public async Task<bool> LogoutAsync(Guid sessionId)
        {
            return await ExecuteSessionActionAsync(
                "dbo.usp_UserSessions_Logout", sessionId);
        }

        public async Task<bool> RevokeAsync(Guid sessionId)
        {
            return await ExecuteSessionActionAsync(
                "dbo.usp_UserSessions_Revoke", sessionId);
        }

        public async Task<List<Session>> GetByUserIdAsync(int userId)
        {
            var sessions = new List<Session>();

            await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_UserSessions_GetByUserId", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sessions.Add(new Session
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    SessionId = reader.GetGuid(reader.GetOrdinal("SessionId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    LoginAt = reader.GetDateTime(reader.GetOrdinal("LoginAt")),
                    LastActivityAt = reader.GetDateTime(reader.GetOrdinal("LastActivityAt")),
                    ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                    LoggedOutAt = reader.IsDBNull(reader.GetOrdinal("LoggedOutAt"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("LoggedOutAt")),
                    IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked")),
                    IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("IpAddress")),
                    UserAgent = reader.IsDBNull(reader.GetOrdinal("UserAgent"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("UserAgent"))
                });
            }

            return sessions;
        }

        private async Task<bool> ExecuteSessionActionAsync(
            string procedureName,
            Guid sessionId)
        {
            await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(procedureName, connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = sessionId;

            var result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToBoolean(result);
        }
    }
}
