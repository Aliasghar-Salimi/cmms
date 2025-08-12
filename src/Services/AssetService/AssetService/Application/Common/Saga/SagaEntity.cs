namespace AssetService.Application.Common.Saga;

public class SagaEntity
{
    public Guid Id { get; set; }
    public Guid SagaId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string SagaType { get; set; } = string.Empty;
    public string SagaData { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTime? NextRetryAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
} 