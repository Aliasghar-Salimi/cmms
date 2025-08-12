namespace AssetService.Application.Common.Saga;

public enum SagaStatus
{
    Started,
    InProgress,
    Completed,
    Failed,
    Compensated
}

public enum SagaStep
{
    ValidateUser,
    ValidateTenant,
    CreateAsset,
    PublishEvent,
    UpdateAuditLog,
    NotifyServices
}

public abstract class BaseSagaState
{
    public Guid SagaId { get; set; } = Guid.NewGuid();
    public string CorrelationId { get; set; } = string.Empty;
    public SagaStatus Status { get; set; } = SagaStatus.Started;
    public List<SagaStep> CompletedSteps { get; set; } = new();
    public List<SagaStep> FailedSteps { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

public class AssetCreationSagaState : BaseSagaState
{
    public Guid AssetId { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public new string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public bool UserValidated { get; set; } = false;
    public bool TenantValidated { get; set; } = false;
    public bool AssetCreated { get; set; } = false;
    public bool EventPublished { get; set; } = false;
    public bool AuditLogUpdated { get; set; } = false;
}

public class AssetUpdateSagaState : BaseSagaState
{
    public Guid AssetId { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public new string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
    public bool UserValidated { get; set; } = false;
    public bool TenantValidated { get; set; } = false;
    public bool AssetUpdated { get; set; } = false;
    public bool EventPublished { get; set; } = false;
    public bool AuditLogUpdated { get; set; } = false;
    
    // Original values for compensation
    public string OriginalAssetName { get; set; } = string.Empty;
    public string OriginalAssetType { get; set; } = string.Empty;
    public string OriginalManufacturer { get; set; } = string.Empty;
    public string OriginalLocation { get; set; } = string.Empty;
    public string OriginalStatus { get; set; } = string.Empty;
    public DateTime OriginalWarrantyExpirationDate { get; set; }
}

public class AssetDeletionSagaState : BaseSagaState
{
    public Guid AssetId { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public bool UserValidated { get; set; } = false;
    public bool TenantValidated { get; set; } = false;
    public bool AssetDeleted { get; set; } = false;
    public bool EventPublished { get; set; } = false;
    public bool AuditLogUpdated { get; set; } = false;
}

// Database entity for saga state persistence
public class SagaStateEntity
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SagaType { get; set; } = string.Empty;
    public string CompletedSteps { get; set; } = string.Empty; // JSON serialized
    public string FailedSteps { get; set; } = string.Empty; // JSON serialized
    public string Errors { get; set; } = string.Empty; // JSON serialized
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public string SagaData { get; set; } = string.Empty; // JSON serialized saga state
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextRetryAt { get; set; }
} 