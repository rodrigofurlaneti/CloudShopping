using CloudShopping.Application.Abstractions.Data;
using MySqlConnector;
using System.Data;

namespace CloudShopping.Infrastructure.Persistence
{
    public sealed class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
