using Microsoft.Extensions.Configuration;
using NotifyHub.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace NotifyHub.Infrastructure.Cache
{
    public class RedisTenantCache
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

        public RedisTenantCache(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis") ??
                throw new InvalidOperationException("Redis connection string is not found.");

            var redis = ConnectionMultiplexer.Connect(connectionString);
            _db = redis.GetDatabase();
        }

        public async Task<Tenant?> GetTenantAsync(Guid tenantId)
        {
            var key = $"tenant:{tenantId}:config";
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<Tenant>(value!);
        }

        public async Task SetTenantAsync(Tenant tenant)
        {
            var key = $"tenant:{tenant.Id}:config";
            var value = JsonSerializer.Serialize(tenant);

            await _db.StringSetAsync(key, value, _ttl);
        }

        public async Task InvalidateAsync(Guid tenantId)
        {
            var key = $"tenant:{tenantId}:config";
            await _db.KeyDeleteAsync(key);
        }

    }
}
