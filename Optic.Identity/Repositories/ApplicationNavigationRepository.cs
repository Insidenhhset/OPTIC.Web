
using System.Data;
using Microsoft.Data.SqlClient;
using Optic.Identity.Data;
using Optic.Identity.Models.Navigation;

namespace Optic.Identity.Repositories
{
    public class ApplicationNavigationRepository
        : IApplicationNavigationRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ApplicationNavigationRepository(
            ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ApplicationNavigationItem>>
            GetAccessibleApplicationsAsync(int userId)
        {
            var applications = new List<ApplicationNavigationItem>();

            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_Navigation_GetAccessibleApplications",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                applications.Add(new ApplicationNavigationItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),

                    ApplicationCode = reader.GetString(
                        reader.GetOrdinal("ApplicationCode")),

                    ApplicationName = reader.GetString(
                        reader.GetOrdinal("ApplicationName")),

                    Description = GetNullableString(reader, "Description"),

                    ModuleAssembly = GetNullableString(
                        reader, "ModuleAssembly"),

                    ModulePath = reader.GetString(
                        reader.GetOrdinal("ModulePath")),

                    Icon = GetNullableString(reader, "Icon"),

                    DisplayOrder = reader.GetInt32(
                        reader.GetOrdinal("DisplayOrder"))
                });
            }

            return applications;
        }

        public async Task<List<ApplicationMenuItem>>
            GetAccessibleMenusAsync(int userId, int applicationId)
        {
            var menus = new List<ApplicationMenuItem>();

            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_Navigation_GetAccessibleMenus",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            command.Parameters.Add("@ApplicationId", SqlDbType.Int).Value =
                applicationId;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                menus.Add(new ApplicationMenuItem
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),

                    ApplicationId = reader.GetInt32(
                        reader.GetOrdinal("ApplicationId")),

                    ParentMenuId = GetNullableInt32(
                        reader, "ParentMenuId"),

                    MenuCode = reader.GetString(
                        reader.GetOrdinal("MenuCode")),

                    MenuName = reader.GetString(
                        reader.GetOrdinal("MenuName")),

                    Description = GetNullableString(
                        reader, "Description"),

                    Route = GetNullableString(reader, "Route"),

                    Icon = GetNullableString(reader, "Icon"),

                    DisplayOrder = reader.GetInt32(
                        reader.GetOrdinal("DisplayOrder")),

                    RequiredPermissionId = GetNullableInt32(
                        reader, "RequiredPermissionId"),

                    ShowOnDashboard = reader.GetBoolean(
                        reader.GetOrdinal("ShowOnDashboard")),

                    ShowInSidebar = reader.GetBoolean(
                        reader.GetOrdinal("ShowInSidebar"))
                });
            }

            return BuildMenuTree(menus);
        }

        public async Task<List<UserApplicationRole>> GetUserRolesAsync(
            int userId)
        {
            var roles = new List<UserApplicationRole>();

            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.usp_Users_GetAssignedRoles",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(new UserApplicationRole
                {
                    ApplicationName = reader.GetString(
                        reader.GetOrdinal("ApplicationName")),

                    RoleName = reader.GetString(
                        reader.GetOrdinal("RoleName"))
                });
            }

            return roles;
        }

        private static List<ApplicationMenuItem> BuildMenuTree(
            List<ApplicationMenuItem> menus)
        {
            var menuLookup = menus.ToDictionary(menu => menu.Id);

            var rootMenus = new List<ApplicationMenuItem>();

            foreach (var menu in menus)
            {
                if (menu.ParentMenuId.HasValue &&
                    menuLookup.TryGetValue(
                        menu.ParentMenuId.Value,
                        out var parent))
                {
                    parent.Children.Add(menu);
                }
                else
                {
                    rootMenus.Add(menu);
                }
            }

            SortMenus(rootMenus);

            return rootMenus;
        }

        private static void SortMenus(
            List<ApplicationMenuItem> menus)
        {
            menus.Sort((a, b) =>
                a.DisplayOrder.CompareTo(b.DisplayOrder));

            foreach (var menu in menus)
            {
                SortMenus(menu.Children);
            }
        }

        private static string? GetNullableString(
            SqlDataReader reader,
            string column)
        {
            var ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        private static int? GetNullableInt32(
            SqlDataReader reader,
            string column)
        {
            var ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetInt32(ordinal);
        }
    }
}
