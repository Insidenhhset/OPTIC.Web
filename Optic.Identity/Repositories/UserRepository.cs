
using System.Data;
using Microsoft.Data.SqlClient;
using Optic.Identity.Data;
using Optic.Identity.Exceptions;
using Optic.Identity.Models;

namespace Optic.Identity.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_GetByUsername", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@Username", SqlDbType.VarChar, 100).Value = username;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl"))
                ? null
                : reader.GetString(reader.GetOrdinal("AvatarUrl")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
        };
    }

    public async Task<int> CreateAsync(User user, string createdBy)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_Manage", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = "INSERT";
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 50).Value = user.EmployeeCode;
        command.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = user.Username;
        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 100).Value = user.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.VarChar, 100).Value = user.LastName;
        command.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = user.Email;

        command.Parameters.Add("@Mobile", SqlDbType.VarChar, 20).Value =
            (object?)user.Mobile ?? DBNull.Value;

        command.Parameters.Add("@AvatarUrl", SqlDbType.VarChar, 500).Value =
            (object?)user.AvatarUrl ?? DBNull.Value;

        command.Parameters.Add("@EmployeeTypeId", SqlDbType.Int).Value = user.EmployeeTypeId;

        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, -1).Value =
            user.PasswordHash;

        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;

        command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value =
            (object?)createdBy ?? DBNull.Value;

        var result = await ExecuteManageCommandAsync(command);

        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException("User creation did not return an ID.");

        return Convert.ToInt32(result);
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_GetAll", connection);

        command.CommandType = CommandType.StoredProcedure;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_UpdateLastLogin", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_GetById", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapUser(reader);
    }

    public async Task<bool> UpdateAsync(User user, string updatedBy)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_Manage", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = "UPDATE";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = user.Id;
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 50).Value = user.EmployeeCode;
        command.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = user.Username;
        command.Parameters.Add("@FirstName", SqlDbType.VarChar, 100).Value = user.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.VarChar, 100).Value = user.LastName;
        command.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = user.Email;

        command.Parameters.Add("@Mobile", SqlDbType.VarChar, 20).Value =
            (object?)user.Mobile ?? DBNull.Value;

        command.Parameters.Add("@AvatarUrl", SqlDbType.VarChar, 500).Value =
            (object?)user.AvatarUrl ?? DBNull.Value;

        command.Parameters.Add("@EmployeeTypeId", SqlDbType.Int).Value = user.EmployeeTypeId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;

        command.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value =
            (object?)updatedBy ?? DBNull.Value;

        var result = await ExecuteManageCommandAsync(command);

        return result != null && result != DBNull.Value;
    }

    public async Task<bool> DeleteAsync(int id, string updatedBy)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.usp_Users_Manage", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = "DELETE";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        command.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value =
            (object?)updatedBy ?? DBNull.Value;

        var result = await ExecuteManageCommandAsync(command);

        return result != null && result != DBNull.Value;
    }

    private static async Task<object?> ExecuteManageCommandAsync(
        SqlCommand command)
    {
        try
        {
            return await command.ExecuteScalarAsync();
        }
        catch (SqlException ex)
        {
            switch (ex.Number)
            {
                // Duplicate Employee Code
                case 50011:
                    throw new ConflictException(
                        "Employee Code already exists.",
                        "EmployeeCode");

                // Duplicate Username
                case 50012:
                    throw new ConflictException(
                        "Username already exists.",
                        "Username");

                // Duplicate Email
                case 50013:
                    throw new ConflictException(
                        "Email already exists.",
                        "Email");

                // Legacy combined duplicate errors
                case 50001:
                case 50003:
                    throw new ConflictException(
                        "One or more user fields already exist.");

                // User not found during update
                case 50002:
                    throw new NotFoundException(
                        "User was not found.");

                // User not found or already deleted
                case 50004:
                    throw new NotFoundException(
                        "User was not found or is already deleted.");

                default:
                    throw;
            }
        }
    }

    private static User MapUser(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),

            Mobile = reader.IsDBNull(reader.GetOrdinal("Mobile"))
                ? null
                : reader.GetString(reader.GetOrdinal("Mobile")),

            AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl"))
                ? null
                : reader.GetString(reader.GetOrdinal("AvatarUrl")),

            EmployeeTypeId = reader.GetInt32(reader.GetOrdinal("EmployeeTypeId")),

            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),

            LastLoginAt = reader.IsDBNull(reader.GetOrdinal("LastLoginAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("LastLoginAt")),

            PasswordChangedAt = reader.IsDBNull(reader.GetOrdinal("PasswordChangedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("PasswordChangedAt")),

            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                ? null
                : reader.GetString(reader.GetOrdinal("CreatedBy")),

            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
                ? null
                : reader.GetString(reader.GetOrdinal("UpdatedBy"))
        };
    }
}
