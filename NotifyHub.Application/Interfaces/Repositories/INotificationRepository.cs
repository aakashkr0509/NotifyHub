using NotifyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByTenantIdAsync(Guid tenantId);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, Guid tenantId);
        Task<Notification?> GetByIdAsync(Guid id, Guid tenantId);
        Task<int> GetUnreadCountAsync(Guid userId, Guid tenantId);
        Task<Guid> CreateAsync(Notification notification);
        Task MarkAsReadAsync(Guid id, Guid tenantId);
        Task MarkAllAsReadAsync(Guid userId, Guid tenantId);
        Task DeleteAsync(Guid id, Guid tenantId);
    }
}
