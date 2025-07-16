using IdentityService.Application.Features.Users.DTOs;

namespace IdentityService.Application.Features.Auth.DTOs;

public class CurrentUserDto
{
    public UserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool HasMfaEnabled { get; set; } = false;
    public string? MfaType { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
} 