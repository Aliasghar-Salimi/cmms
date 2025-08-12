namespace AssetService.Application.Common.Saga;

using AssetService.Application.Common.Services;
using AssetService.Application.Common.SharedModels;

public interface ISagaOrchestrator
{
    Task<SagaResult> ExecuteCreateAssetSagaAsync(CreateAssetSagaRequest request, CancellationToken cancellationToken = default);
    Task<SagaResult> ExecuteUpdateAssetSagaAsync(UpdateAssetSagaRequest request, CancellationToken cancellationToken = default);
    Task<SagaResult> ExecuteDeleteAssetSagaAsync(DeleteAssetSagaRequest request, CancellationToken cancellationToken = default);
    Task<SagaResult> CompensateAsync(string sagaId, CancellationToken cancellationToken = default);
}

public class CreateAssetSagaRequest
{
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? WarrantyExpirationDate { get; set; }
    public string UserToken { get; set; } = string.Empty;
}

public class UpdateAssetSagaRequest
{
    public Guid AssetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? WarrantyExpirationDate { get; set; }
    public string UserToken { get; set; } = string.Empty;
}

public class DeleteAssetSagaRequest
{
    public Guid AssetId { get; set; }
    public string UserToken { get; set; } = string.Empty;
}

public class SagaResult
{
    public bool IsSuccess { get; set; }
    public string SagaId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<SagaStepResult> Steps { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class SagaStepResult
{
    public string StepName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime CompletedAt { get; set; }
} 