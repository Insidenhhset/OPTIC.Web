
using System.Data;
using Microsoft.Data.SqlClient;
using Optic.Identity.Data;
using Optic.Identity.Models.RoleManagement;

namespace Optic.Identity.Repositories
{
    public class RoleManagementRepository : IRoleManagementRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public RoleManagementRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<RoleManagementDashboardStats> GetDashboardStatsAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_RoleManagement_GetDashboardStats",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException(
                    "Role management dashboard statistics were not returned.");
            }

            return new RoleManagementDashboardStats
            {
                TotalRoles = reader.GetInt32(
                    reader.GetOrdinal("TotalRoles")),

                TotalApplicationMappings = reader.GetInt32(
                    reader.GetOrdinal("TotalApplicationMappings")),

                ActiveAssignments = reader.GetInt32(
                    reader.GetOrdinal("ActiveAssignments"))
            };
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = new List<Role>();

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_RoleManagement_GetAllRoles",connection);

            command.CommandType = CommandType.StoredProcedure;
            await using var reader = await command.ExecuteReaderAsync();

            var idOrdinal = reader.GetOrdinal("Id");
            var roleCodeOrdinal = reader.GetOrdinal("RoleCode");
            var roleNameOrdinal = reader.GetOrdinal("RoleName");
            var descriptionOrdinal = reader.GetOrdinal("Description");
            var isSystemOrdinal = reader.GetOrdinal("IsSystem");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");
            var createdAtOrdinal = reader.GetOrdinal("CreatedAt");
            var createdByOrdinal = reader.GetOrdinal("CreatedBy");
            var updatedAtOrdinal = reader.GetOrdinal("UpdatedAt");
            var updatedByOrdinal = reader.GetOrdinal("UpdatedBy");

            while (await reader.ReadAsync())
            {
                roles.Add(new Role
                {
                    Id = reader.GetInt32(idOrdinal),
                    RoleCode = reader.GetString(roleCodeOrdinal),
                    RoleName = reader.GetString(roleNameOrdinal),
                    Description = reader.IsDBNull(descriptionOrdinal)
                        ? null : reader.GetString(descriptionOrdinal),
                    IsSystem = reader.GetBoolean(isSystemOrdinal),
                    IsActive = reader.GetBoolean(isActiveOrdinal),
                    CreatedAt = reader.GetDateTime(createdAtOrdinal),
                    CreatedBy = reader.IsDBNull(createdByOrdinal)
                        ? null : reader.GetInt32(createdByOrdinal),
                    UpdatedAt = reader.IsDBNull(updatedAtOrdinal)
                        ? null : reader.GetDateTime(updatedAtOrdinal),
                    UpdatedBy = reader.IsDBNull(updatedByOrdinal)
                        ? null : reader.GetInt32(updatedByOrdinal)
                });
            }

            return roles;
        }
        public async Task<int> CreateRoleAsync(Role role, int? createdBy)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_RoleManagement_CreateRole",connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@RoleCode", SqlDbType.VarChar, 50)
                .Value = role.RoleCode.Trim();

            command.Parameters.Add("@RoleName", SqlDbType.VarChar, 100)
                .Value = role.RoleName.Trim();

            command.Parameters.Add("@Description", SqlDbType.VarChar, 500)
                .Value = string.IsNullOrWhiteSpace(role.Description)
                    ? DBNull.Value
                    : role.Description.Trim();
            command.Parameters.Add("@IsSystem", SqlDbType.Bit).Value = role.IsSystem;
            command.Parameters.Add("@CreatedBy", SqlDbType.Int)
                .Value = createdBy.HasValue
                    ? createdBy.Value
                    : DBNull.Value;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.usp_RoleManagement_GetRoleById",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Role
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                RoleCode = reader.GetString(reader.GetOrdinal("RoleCode")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                Description = reader["Description"] as string,
                IsSystem = reader.GetBoolean(reader.GetOrdinal("IsSystem")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedBy = reader["CreatedBy"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["CreatedBy"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["UpdatedAt"]),
                UpdatedBy = reader["UpdatedBy"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["UpdatedBy"])
            };
        }


        public async Task<bool> UpdateRoleAsync(Role role, int? updatedBy)
        {
            using var connection =_connectionFactory.CreateConnection();

            using var command = new SqlCommand(
                "dbo.usp_RoleManagement_UpdateRole",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Id", SqlDbType.Int).Value = role.Id;

            command.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value =
                role.RoleName.Trim();

            command.Parameters.Add("@Description", SqlDbType.VarChar, 500).Value =
                string.IsNullOrWhiteSpace(role.Description)
                    ? DBNull.Value
                    : role.Description.Trim();

            command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value =
                updatedBy.HasValue ? updatedBy.Value : DBNull.Value;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> UpdateRoleStatusAsync(int id, bool isActive, int? updatedBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "dbo.usp_RoleManagement_UpdateRoleStatus",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
            command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value =
                (object?)updatedBy ?? DBNull.Value;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<List<ApplicationRoleMapping>> GetApplicationRoleMappingsAsync(int applicationId)
        {
            var mappings = new List<ApplicationRoleMapping>();

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "dbo.usp_RoleManagement_GetApplicationRoleMappings",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = applicationId;

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                mappings.Add(new ApplicationRoleMapping
                {
                    ApplicationId = reader.GetInt32(reader.GetOrdinal("ApplicationId")),
                    ApplicationCode = reader.GetString(reader.GetOrdinal("ApplicationCode")),
                    ApplicationName = reader.GetString(reader.GetOrdinal("ApplicationName")),
                    ApplicationIsActive = reader.GetBoolean(reader.GetOrdinal("ApplicationIsActive")),

                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    RoleCode = reader.GetString(reader.GetOrdinal("RoleCode")),
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                    RoleDescription = reader["RoleDescription"] as string,
                    IsSystem = reader.GetBoolean(reader.GetOrdinal("IsSystem")),
                    RoleIsActive = reader.GetBoolean(reader.GetOrdinal("RoleIsActive")),

                    MappingId = reader["MappingId"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["MappingId"]),

                    IsMapped = reader.GetBoolean(reader.GetOrdinal("IsMapped")),

                    MappingIsActive = reader["MappingIsActive"] == DBNull.Value
                        ? null
                        : Convert.ToBoolean(reader["MappingIsActive"]),

                    MappingCreatedAt = reader["MappingCreatedAt"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["MappingCreatedAt"]),

                    MappingCreatedBy = reader["MappingCreatedBy"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["MappingCreatedBy"])
                });
            }

            return mappings;
        }


        public async Task<int> CreateApplicationRoleMappingAsync(int applicationId,int roleId,int? createdBy)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_RoleManagement_CreateApplicationRoleMapping",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = applicationId;
            command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
            command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value =
                createdBy.HasValue ? createdBy.Value : DBNull.Value;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        public async Task<bool> DeactivateApplicationRoleMappingAsync(int mappingId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_RoleManagement_DeactivateApplicationRoleMapping",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@MappingId", SqlDbType.Int).Value = mappingId;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> DeleteApplicationRoleMappingAsync(int mappingId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_RoleManagement_DeleteApplicationRoleMapping",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@MappingId", SqlDbType.Int).Value = mappingId;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> ActivateApplicationRoleMappingAsync(int mappingId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_RoleManagement_ActivateApplicationRoleMapping",connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@MappingId", SqlDbType.Int).Value = mappingId;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<long> CreateUserRoleAssignmentAsync(int userId,int applicationId,int roleId,DateTime validFrom,DateTime? validTo,int? createdBy)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("dbo.usp_UserRoleAssignment_Create", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = applicationId;
            command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
            command.Parameters.Add("@ValidFrom", SqlDbType.DateTime2).Value = validFrom;
            command.Parameters.Add("@ValidTo", SqlDbType.DateTime2).Value = (object?)validTo ?? DBNull.Value;
            command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = (object?)createdBy ?? DBNull.Value;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        public async Task<DataTable> GetAllUserRoleAssignmentsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand(
                "dbo.usp_UserRoleAssignment_GetAll", connection);

            command.CommandType = CommandType.StoredProcedure;

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();

            await connection.OpenAsync();

            adapter.Fill(dataTable);

            return dataTable;
        }

        public async Task<DataTable> GetEligibleUsersForRoleAssignmentAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("dbo.usp_UserRoleAssignment_GetEligibleUsers", connection);

            command.CommandType = CommandType.StoredProcedure;

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();

            await connection.OpenAsync();
            adapter.Fill(dataTable);

            return dataTable;
        }
    }
}
