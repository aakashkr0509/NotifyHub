using NotifyHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Domain.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Viewer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
