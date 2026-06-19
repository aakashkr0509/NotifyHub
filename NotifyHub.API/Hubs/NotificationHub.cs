using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotifyHub.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinTenantGroup()
        {
            var tenantId = Context.User?
                .FindFirst("tenant_id")?.Value;

            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tenantId))
                throw new HubException(
                    "Tenant ID not found in token.");

            // Join tenant-wide group (for broadcasts)
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"tenant_{tenantId}");

            // Join personal group (for targeted notifications)
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"user_{userId}");
            }
        }

        public async Task LeaveGroup()
        {
            var tenantId = Context.User?
                .FindFirst("tenant_id")?.Value;

            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"tenant_{tenantId}");
            }

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"user_{userId}");
            }
        }

        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine(
                $"Client disconnected: {userId} " +
                $"| ConnectionId: {Context.ConnectionId}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
