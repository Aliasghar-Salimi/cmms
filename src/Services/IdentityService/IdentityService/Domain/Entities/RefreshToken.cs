namespace IdentityService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

[Index(nameof(Token), IsUnique = true)]
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set;}
    public string CreatedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; }
}