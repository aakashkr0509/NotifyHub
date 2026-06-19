using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotifyHub.Application.DTOs;
using NotifyHub.Application.Interfaces;
using NotifyHub.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

using LoginRequest = NotifyHub.Application.DTOs.LoginRequest;
using RegisterRequest = NotifyHub.Application.DTOs.RegisterRequest;

namespace NotifyHub.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;


        public AuthController(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow;
            _config = config;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            //1 - find tenant by subdomain
            var tenant = await _uow.Tenant.GetBySubdomainAsync(request.Subdomain);

            if (tenant is null || !tenant.IsActive)
                return Unauthorized("Invalid tenant.");

            //2 - find user within the tenant
            var user = await _uow.User.GetByEmailAsync(request.Email, tenant.Id);

            if(user is null)
                return Unauthorized("Invalid credentials.");

            //3 - verify password
            var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if(!passwordValid)
                return Unauthorized("Invalid credentials.");

            //4 - generate JWT
            var token = GenerateJwt(user, tenant.Id);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                RefreshToken = Guid.NewGuid().ToString(),
                Email = user.Email,
                Role  = user.Role.ToString(),
                TenantId = tenant.Id,
            });
        }

        private string GenerateJwt(AppUser user, Guid tenantId)
        {
            var secret = _config["Jwt:Secret"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var expiry = int.Parse(
                _config["Jwt:AccessTokenExpiryMinutes"]!);

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,       user.Id.ToString()),
                new Claim(ClaimTypes.Email,     user.Email),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim(ClaimTypes.Role,      user.Role.ToString())
            };

            var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime
                .UtcNow.AddMinutes(expiry),
            signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Seed endpoint — creates a tenant + admin user
        // Remove this in production
        [HttpPost("seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            // Check if tenant already exists
            var existing = await _uow.Tenant
                .GetBySubdomainAsync("acme");

            if (existing is not null)
                return BadRequest(new
                {
                    Message = "Seed already ran. " +
                              "Tenant 'acme' already exists.",
                    TenantId = existing.Id
                });

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Acme Corp",
                Subdomain = "acme",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Tenant.CreateAsync(tenant);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = "admin@acme.com",
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword("Admin@123"),
                Role = Domain.Enums.UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.User.CreateAsync(user);

            return Ok(new
            {
                Message = "Seed successful",
                Tenant = tenant.Subdomain,
                TenantId = tenant.Id,
                Email = user.Email,
                Password = "Admin@123"
            });
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
        {
            // Find tenant first
            var tenant = await _uow.Tenant
                .GetBySubdomainAsync(request.Subdomain);

            if (tenant is null)
                return BadRequest("Tenant not found.");

            // Check if email already exists
            var existing = await _uow.User
                .GetByEmailAsync(request.Email, tenant.Id);

            if (existing is not null)
                return BadRequest("Email already registered.");

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,       // ← same tenant
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt
                    .HashPassword(request.Password),
                Role = Domain.Enums.UserRole.Viewer,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.User.CreateAsync(user);

            return Ok(new
            {
                Message = "User registered successfully",
                UserId = user.Id,
                TenantId = tenant.Id,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }
    }
}
