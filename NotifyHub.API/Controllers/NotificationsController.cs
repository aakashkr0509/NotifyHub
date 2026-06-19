using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyHub.Application.Services;
using NotifyHub.API.BackgroundServices;
using NotifyHub.Application.DTOs;
using System.Threading.Channels;
using System.Security.Claims;

namespace NotifyHub.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;
        private readonly Channel<NotificationJob> _channel;

        public NotificationsController(NotificationService service, Channel<NotificationJob> channel)
        {
            _service = service;
            _channel = channel;
        }

        //Helper - reads tenant Id injected by MiddleWare
        private Guid TenantId => (Guid)HttpContext.Items["TenantId"];

        //helper - reads user Id form JWT claim
        private Guid UserId => Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications = await _service.GetForUserAsync(UserId, TenantId);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _service.GetUnreadCountAsync(UserId, TenantId);
            return Ok(new { count });

        }

        [HttpPost]
        public async Task<IActionResult> Create(
        [FromBody] CreateNotificationRequest request)
        {
            var notification = await _service
                .CreateAsync(request, TenantId);

            // Pass target user id so worker knows
            // whether to broadcast or target
            await _channel.Writer.WriteAsync(new NotificationJob
            {
                TenantId = TenantId.ToString(),
                TargetUserId = request.UserId?.ToString(),
                Notification = notification
            });

            return CreatedAtAction(
                nameof(GetAll), notification);
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _service.MarkAsReadAsync(id, TenantId);
            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _service.MarkAllAsReadAsync(UserId, TenantId);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id, TenantId);
            return NoContent();
        }
    }
}
