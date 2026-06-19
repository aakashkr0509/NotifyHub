using NotifyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Application.Interfaces.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetBySubdomainAsync(string subdomain);
        Task<Guid> CreateAsync(Tenant tenant);
    }
}
