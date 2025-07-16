using IdentityService.Application.Features.Users.DTOs;

namespace IdentityService.Application.Features.Auth.DTOs;

public class MfaLoginResponseDto
{
    public bool RequiresMfa { get; set; } = false;
    public string? MfaToken { get; set; } // Temporary token for MFA verification
    public string? PhoneNumber { get; set; } // Masked phone number for SMS
    public string MfaType { get; set; } = "sms"; // "sms", "email", "totp"
    public DateTime ExpiresAt { get; set; }
    public string? AccessToken { get; set; } // Only provided after MFA verification
    public string? RefreshToken { get; set; } // Only provided after MFA verification
    public UserDto? User { get; set; } // Only provided after MFA verification
    public List<string>? Roles { get; set; } // Only provided after MFA verification
    public List<string>? Permissions { get; set; } // Only provided after MFA verification
} 