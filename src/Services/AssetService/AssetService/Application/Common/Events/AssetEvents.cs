namespace AssetService.Application.Common.Events;

public abstract class BaseEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = "AssetService";
    public string Version { get; set; } = "1.0";
}

public class AssetCreatedEvent : BaseEvent
{
    public AssetCreatedEvent()
    {
        EventType = "AssetCreated";
    }
    
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
}

public class AssetUpdatedEvent : BaseEvent
{
    public AssetUpdatedEvent()
    {
        EventType = "AssetUpdated";
    }
    
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
}

public class AssetDeletedEvent : BaseEvent
{
    public AssetDeletedEvent()
    {
        EventType = "AssetDeleted";
    }
    
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid DeletedByUserId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AssetStatusChangedEvent : BaseEvent
{
    public AssetStatusChangedEvent()
    {
        EventType = "AssetStatusChanged";
    }
    
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
} 