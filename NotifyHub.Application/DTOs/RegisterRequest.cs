namespace NotifyHub.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
}