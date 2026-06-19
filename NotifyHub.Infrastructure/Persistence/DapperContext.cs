using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;


namespace NotifyHub.Infrastructure.Persistence
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgreSQL")
                                ?? throw new InvalidOperationException("PostgreSQL connection string not found.");
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}
