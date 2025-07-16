namespace IdentityService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

[Index(nameof(UserId), IsUnique = true)]
public class UserMfa
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsEnabled { get; set; } = false;
    public string MfaType { get; set; } = "sms"; // "sms", "email", "totp", etc.
    public string? BackupPhoneNumber { get; set; }
    public string? BackupEmail { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // For TOTP (Time-based One-Time Password)
    public string? TotpSecret { get; set; }
    public bool TotpEnabled { get; set; } = false;
    
    // For backup codes
    public string? BackupCodes { get; set; } // JSON array of backup codes
    public int BackupCodesRemaining { get; set; } = 0;
} 