using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Domain.Entities;

[Index(nameof(Id))]
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; }
    public string? EntityName { get; set; } 
    public Guid? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? DataBefore { get; set; }
    public string? DataAfter { get; set; }
    public string? CorrelationId { get; set; }
    public string? MetaData { get; set; }
}
// use cases
// On Create / Update / Delete

// On Login / Logout / Access Denied

// On Permission or Role change

// On API access to protected resources