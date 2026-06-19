using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotifyHub.API.BackgroundServices;
using NotifyHub.API.Hubs;
using NotifyHub.API.Middleware;
using NotifyHub.Infrastructure;
using NotifyHub.Infrastructure.Persistence;
using StackExchange.Redis;
using System.Text;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);
// Controllers
builder.Services.AddControllers();
// Infrastructure (Dapper, Redis, UnitOfWork, Services)
builder.Services.AddInfrastructure(builder.Configuration);
// Channel for background notification queue
builder.Services.AddSingleton(Channel.CreateUnbounded<NotificationJob>());
// Background worker
builder.Services.AddHostedService<NotificationWorker>();
// SignalR + Redis backplane
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!);

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey =
                    new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization();

// CORS — allow Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("Angular");
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
