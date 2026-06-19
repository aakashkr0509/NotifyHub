using Microsoft.AspNetCore.SignalR;
using NotifyHub.Application.DTOs;
using System.Threading.Channels;
using NotifyHub.API.Hubs;

namespace NotifyHub.API.BackgroundServices
{
    public class NotificationJob
    {
        public string TenantId { get; set; } = string.Empty;
        public string? TargetUserId { get; set; }
        public NotificationDto Notification { get; set; } = new();
    }
    public class NotificationWorker : BackgroundService
    {
        private readonly Channel<NotificationJob> _channel;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationWorker> _logger;
        
        public NotificationWorker(Channel<NotificationJob> channel, IHubContext<NotificationHub> hubContext,ILogger<NotificationWorker> logger)
        {
            _channel = channel;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
     CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker started.");

            await foreach (var job in _channel.Reader
                .ReadAllAsync(stoppingToken))
            {
                try
                {
                    if (string.IsNullOrEmpty(job.TargetUserId))
                    {
                        // Broadcast — send to entire tenant group
                        await _hubContext.Clients
                            .Group($"tenant_{job.TenantId}")
                            .SendAsync(
                                "ReceiveNotification",
                                job.Notification,
                                stoppingToken);
                    }
                    else
                    {
                        // Targeted — send only to specific user group
                        await _hubContext.Clients
                            .Group($"user_{job.TargetUserId}")
                            .SendAsync(
                                "ReceiveNotification",
                                job.Notification,
                                stoppingToken);
                    }

                    _logger.LogInformation(
                        "Notification dispatched to " +
                        "tenant {TenantId}", job.TenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to dispatch notification " +
                        "to tenant {TenantId}", job.TenantId);
                }
            }
        }
    }
}
