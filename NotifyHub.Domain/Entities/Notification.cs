using NotifyHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? UserId { get; set; }        // null = broadcast to all tenant users
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
