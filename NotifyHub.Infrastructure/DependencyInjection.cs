using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Services;
using NotifyHub.Infrastructure.Cache;
using NotifyHub.Infrastructure.Persistence;

namespace NotifyHub.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
        {
            services.AddSingleton<DapperContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<RedisTenantCache>();
            services.AddScoped<NotificationService>();

            return services;
        }
    }
}
