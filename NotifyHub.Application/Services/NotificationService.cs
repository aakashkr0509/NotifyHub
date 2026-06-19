using NotifyHub.Application.DTOs;
using NotifyHub.Application.Interfaces;
using NotifyHub.Domain.Entities;
using NotifyHub.Domain.Enums;

namespace NotifyHub.Application.Services
{
    public class NotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<NotificationDto>> GetForUserAsync(Guid userId, Guid tenantId)
        {
            var notifications = await _uow.Notifications.GetByUserIdAsync(userId, tenantId);

            return notifications.Select(MapToDto);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, Guid tenantId) 
        {
            return await _uow.Notifications.GetUnreadCountAsync(userId, tenantId);
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, Guid tenantId)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = request.UserId,
                Title = request.Title,
                Body = request.Body,
                Status = NotificationStatus.Unread,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Notifications.CreateAsync(notification);
            return MapToDto(notification);
        }

        public async Task DeleteAsync(Guid id, Guid tenantId)
        {
            await _uow.Notifications.DeleteAsync(id, tenantId);
        }

        public async Task MarkAsReadAsync(Guid id, Guid tenantId)
        {
            await _uow.Notifications.MarkAsReadAsync(id, tenantId);
        }
        public async Task MarkAllAsReadAsync(Guid userId, Guid tenantId)
        {
            await _uow.Notifications.MarkAllAsReadAsync(userId, tenantId);
        }



        public static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            TenantId = n.TenantId,
            UserId = n.UserId,
            Title = n.Title,
            Body = n.Body,
            Status = n.Status.ToString(),
            CreatedAt = n.CreatedAt
        };
    }
}
