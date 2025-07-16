namespace IdentityService.Domain.Entities;

using Microsoft.EntityFrameworkCore;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
// use cases
// On Create / Update / Delete

// On Login / Logout / Access Denied

// On Permission or Role change

// On API access to protected resources