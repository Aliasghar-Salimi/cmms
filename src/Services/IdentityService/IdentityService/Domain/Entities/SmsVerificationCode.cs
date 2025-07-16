namespace IdentityService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

[Index(nameof(PhoneNumber), nameof(Code), IsUnique = false)]
[Index(nameof(ExpiresAt))]
public class SmsVerificationCode
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // "otp", "password-reset", "mfa", etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string? UsedByIp { get; set; }
    public int Attempts { get; set; } = 0;
    public int MaxAttempts { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool CanBeUsed => IsActive && !IsUsed && !IsExpired && Attempts < MaxAttempts;
} 