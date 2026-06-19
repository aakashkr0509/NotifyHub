using NotifyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email, Guid tenantId);
        Task<AppUser?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(AppUser user); 
    }
}
