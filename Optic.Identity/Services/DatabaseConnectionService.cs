
using Optic.Identity.Data;

namespace Optic.Identity.Services
{
    public class DatabaseConnectionService
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DatabaseConnectionService(
            ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<string> TestConnectionAsync()
        {
            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT DB_NAME()";

            var databaseName = await command.ExecuteScalarAsync();

            return $"Successfully connected to database: {databaseName}";
        }
    }
}
