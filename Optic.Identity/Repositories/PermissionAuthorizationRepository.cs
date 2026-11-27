using System.Data;
using Microsoft.Data.SqlClient;
using Optic.Identity.Data;
using Optic.Identity.Repositories.Interfaces;

namespace Optic.Identity.Repositories
{
    public class PermissionAuthorizationRepository : IPermissionAuthorizationRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public PermissionAuthorizationRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> HasPermissionAsync(int userId,string applicationCode,string permissionCode)
        {
            await using var connection = _connectionFactory.CreateConnection();

            await using var command = new SqlCommand("dbo.usp_Authorization_HasPermission",connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@ApplicationCode",SqlDbType.NVarChar,50).Value = applicationCode;
            command.Parameters.Add("@PermissionCode",SqlDbType.NVarChar,100).Value = permissionCode;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return result != null && result != DBNull.Value && Convert.ToBoolean(result);
        }
    }
}