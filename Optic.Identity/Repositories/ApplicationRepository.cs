using Microsoft.Data.SqlClient;
using System.Data;
using Optic.Identity.Models.RoleManagement;
using Optic.Identity.Data;

namespace Optic.Identity.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ApplicationRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Application>> GetAllApplicationsAsync()
        {
            var applications = new List<Application>();

            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.usp_Applications_GetAll",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                applications.Add(new Application
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    ApplicationCode = reader.GetString(reader.GetOrdinal("ApplicationCode")),
                    ApplicationName = reader.GetString(reader.GetOrdinal("ApplicationName")),
                    Description = reader["Description"] == DBNull.Value
                        ? null
                        : reader.GetString(reader.GetOrdinal("Description")),
                    IsSystem = reader.GetBoolean(reader.GetOrdinal("IsSystem")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                });
            }

            return applications;
        }
    }
}