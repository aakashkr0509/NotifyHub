using NotifyHub.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INotificationRepository Notifications { get; }
        IUserRepository User { get; }
        ITenantRepository Tenant { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
