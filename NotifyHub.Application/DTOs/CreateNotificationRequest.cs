using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifyHub.Application.DTOs
{
    public class CreateNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // null means broadcast to all users in the tenant
        public Guid? UserId { get; set; }
    }
}
