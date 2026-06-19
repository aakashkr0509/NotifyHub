using Dapper;
using NotifyHub.Application.Interfaces.Repositories;
using NotifyHub.Domain.Entities;
using NotifyHub.Domain.Enums;
using System.Data;

namespace NotifyHub.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<AppUser?> GetByEmailAsync(string email, Guid tenantId)
        {
            const string sql = @"Select id,tenant_id, email, password_hash, role, created_at from app_users where email = @Email and
                                    tenant_id = @TenantId";

            var row = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Email = email, TenantId = tenantId });

            return row is null ? null : MapRow(row);
        }

        public async Task<AppUser?> GetByIdAsync(Guid id) 
        {
            const string sql = @"Select id, tenant_id, email, password_hash, role, created_at from 
                                app_users where id = @Id";

            var row = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                sql, new { Id = id });

            return row is null ? null : MapRow(row);

        }

        public async Task<Guid> CreateAsync(AppUser user)
        {
            const string sql = @"Insert into app_users(id, tenant_id, email, password_hash,
                                    role,created_at) Values (@Id, @TenantId, @Email, @PasswordHash, @Role, @CreatedAt) Returning id";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                user.Id,
                user.TenantId,
                user.Email,
                user.PasswordHash,
                Role = user.Role.ToString(),
                user.CreatedAt
            });
        }

        private static AppUser MapRow(dynamic row) => new()
        {
            Id = row.id,
            TenantId = row.tenant_id,
            Email = row.email,
            PasswordHash = row.password_hash,
            Role = Enum.Parse<UserRole>((string)row.role),
            CreatedAt = row.created_at
        };
    }
}
