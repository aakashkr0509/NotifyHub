using Dapper;
using NotifyHub.Application.Interfaces.Repositories;
using NotifyHub.Domain.Entities;
using System.Data;

namespace NotifyHub.Infrastructure.Persistence.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IDbConnection _connection;

        public TenantRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id)
        {
            const string sql = @"Select id, name, subdomain, is_active, created_at from tenants where id = @Id";

            var row = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                sql, new { Id = id });

            return row is null ? null : MapRow(row);
        }

        public async Task<Tenant?> GetBySubdomainAsync(string subdomain) 
        {
            const string sql = @"SELECT id, name, subdomain, is_active, created_at
            FROM tenants
            WHERE subdomain = @Subdomain";

            var row = await _connection.QueryFirstOrDefaultAsync<dynamic>(
            sql, new { Subdomain = subdomain });

            return row is null ? null : MapRow(row);
        }

        public async Task<Guid> CreateAsync(Tenant tenant)
        {
            const string sql = @"
            INSERT INTO tenants
                (id, name, subdomain, is_active, created_at)
            VALUES
                (@Id, @Name, @Subdomain, @IsActive, @CreatedAt)
            RETURNING id";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.IsActive,
                tenant.CreatedAt
            });
        }

        private static Tenant MapRow(dynamic row) => new()
        {
            Id = row.id,
            Name = row.name,
            Subdomain = row.subdomain,
            IsActive = row.is_active,
            CreatedAt = row.created_at
        };

    }
}
