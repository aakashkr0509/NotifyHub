using System.Security.Claims;

namespace NotifyHub.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Only process authenticated requests
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;

                if(!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    context.Items["TenantId"] = tenantId;
                }
            }

            await _next(context);
        }
    }
}
