using System;
using Dapper;
using NotifyHub.Application.Interfaces.Repositories;
using NotifyHub.Domain.Entities;
using NotifyHub.Domain.Enums;
using System.Data;

namespace NotifyHub.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        public readonly IDbConnection _connection;

        public NotificationRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Notification>> GetByTenantIdAsync(Guid tenantId)
        {
            const string sql = @"Select id,tenant_id,user_id,title,body,status,created_at
                                from notifications where tenant_id = @TenantId Order By Created_at desc";

            var rows = await _connection.QueryAsync<dynamic>(sql, new { TenantId = tenantId });

            return rows.Select(MapRow);
                
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(
     Guid userId, Guid tenantId)
        {
            const string sql = @"
        SELECT id, tenant_id, user_id, title,
               body, status, created_at
        FROM notifications
        WHERE tenant_id = @TenantId
          AND (
                user_id = @UserId      -- targeted at me
                OR user_id IS NULL     -- broadcast to all
              )
        ORDER BY created_at DESC";

            var rows = await _connection.QueryAsync<dynamic>(
                sql, new { TenantId = tenantId, UserId = userId });

            return rows.Select(MapRow);
        }

        public async Task<Notification?> GetByIdAsync(Guid id, Guid tenantId)
        {
            const string sql = @"
            SELECT id, tenant_id, user_id, title,
                   body, status, created_at
            FROM notifications
            WHERE id = @Id
              AND tenant_id = @TenantId";

            var row = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                sql, new { Id = id, TenantId = tenantId });

            return row is null ? null : MapRow(row);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, Guid tenantId)
        {
            const string sql = @"
            SELECT COUNT(*) FROM notifications
            WHERE tenant_id = @TenantId
              AND (user_id = @UserId OR user_id IS NULL)
              AND status = 'Unread'";

            return await _connection.ExecuteScalarAsync<int>(
                sql, new { TenantId = tenantId, UserId = userId });
        }

        public async Task<Guid> CreateAsync(Notification notification)
        {
            const string sql = @"
            INSERT INTO notifications 
                (id, tenant_id, user_id, title, body, status, created_at)
            VALUES 
                (@Id, @TenantId, @UserId, @Title, @Body, @Status, @CreatedAt)
            RETURNING id";

            return await _connection.ExecuteScalarAsync<Guid>(sql, new
            {
                notification.Id,
                notification.TenantId,
                notification.UserId,
                notification.Title,
                notification.Body,
                Status = notification.Status.ToString(),
                notification.CreatedAt
            });
        }

        public async Task MarkAsReadAsync(Guid id, Guid tenantId)
        {
            const string sql = @"
            UPDATE notifications
            SET status = 'Read'
            WHERE id = @Id
              AND tenant_id = @TenantId";

            await _connection.ExecuteAsync(
                sql, new { Id = id, TenantId = tenantId });
        }

        public async Task MarkAllAsReadAsync(Guid userId, Guid tenantId)
        {
            const string sql = @"
            UPDATE notifications
            SET status = 'Read'
            WHERE tenant_id = @TenantId
              AND (user_id = @UserId OR user_id IS NULL)
              AND status = 'Unread'";

            await _connection.ExecuteAsync(
                sql, new { UserId = userId, TenantId = tenantId });
        }

        public async Task DeleteAsync(Guid id, Guid tenantId)
        {
            const string sql = @"
            DELETE FROM notifications
            WHERE id = @Id
              AND tenant_id = @TenantId";

            await _connection.ExecuteAsync(
                sql, new { Id = id, TenantId = tenantId });
        }

        // Maps dynamic Dapper row to strongly typed entity
        // We do this manually because column names use
        // snake_case but C# properties use PascalCase
        private static Notification MapRow(dynamic row) => new()
        {
            Id = row.id,
            TenantId = row.tenant_id,
            UserId = row.user_id,
            Title = row.title,
            Body = row.body,
            Status = Enum.Parse<NotificationStatus>((string)row.status),
            CreatedAt = row.created_at
        };
}
}
