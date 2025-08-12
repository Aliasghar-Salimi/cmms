namespace AssetService.Application.Common.Saga;

public interface ISagaStateRepository
{
    Task<SagaStateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SagaStateEntity?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SagaStateEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<SagaStateEntity>> GetBySagaTypeAsync(string sagaType, CancellationToken cancellationToken = default);
    Task<SagaStateEntity> SaveAsync(SagaStateEntity sagaState, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<bool> IncrementRetryCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetNextRetryAsync(Guid id, DateTime nextRetryAt, CancellationToken cancellationToken = default);
    Task<IEnumerable<SagaStateEntity>> GetFailedSagasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SagaStateEntity>> GetSagasForRetryAsync(CancellationToken cancellationToken = default);
    Task<SagaStateEntity> AddAsync(SagaEntity sagaEntity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(SagaEntity sagaEntity, CancellationToken cancellationToken = default);
} 