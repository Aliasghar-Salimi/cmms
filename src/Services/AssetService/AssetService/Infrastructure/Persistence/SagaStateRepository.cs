using AssetService.Application.Common.Saga;
using AssetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AssetService.Infrastructure.Persistence;

public class SagaStateRepository : ISagaStateRepository
{
    private readonly AssetServiceDbContext _context;

    public SagaStateRepository(AssetServiceDbContext context)
    {
        _context = context;
    }

    public async Task<SagaStateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SagaStateEntity?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<IEnumerable<SagaStateEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SagaStateEntity>> GetBySagaTypeAsync(string sagaType, CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .Where(s => s.SagaType == sagaType)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SagaStateEntity> SaveAsync(SagaStateEntity sagaState, CancellationToken cancellationToken = default)
    {
        if (sagaState.Id == Guid.Empty)
        {
            sagaState.Id = Guid.NewGuid();
            sagaState.CreatedAt = DateTime.UtcNow;
            _context.SagaStates.Add(sagaState);
        }
        else
        {
            sagaState.UpdatedAt = DateTime.UtcNow;
            _context.SagaStates.Update(sagaState);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sagaState;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var sagaState = await _context.SagaStates.FindAsync(id, cancellationToken);
        if (sagaState == null)
            return false;

        sagaState.Status = status;
        sagaState.UpdatedAt = DateTime.UtcNow;
        
        if (status == "Completed" || status == "Failed" || status == "Compensated")
        {
            sagaState.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IncrementRetryCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var sagaState = await _context.SagaStates.FindAsync(id, cancellationToken);
            if (sagaState != null)
            {
                sagaState.RetryCount++;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetNextRetryAsync(Guid id, DateTime nextRetryAt, CancellationToken cancellationToken = default)
    {
        try
        {
            var sagaState = await _context.SagaStates.FindAsync(id, cancellationToken);
            if (sagaState != null)
            {
                sagaState.NextRetryAt = nextRetryAt;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<SagaStateEntity>> GetFailedSagasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .Where(s => s.Status == "Failed" && s.RetryCount < s.MaxRetries)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SagaStateEntity>> GetStaleSagasAsync(TimeSpan threshold, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(threshold);
        return await _context.SagaStates
            .Where(s => s.Status == "InProgress" && s.StartedAt < cutoffTime)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SagaStateEntity>> GetSagasForRetryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SagaStates
            .Where(s => s.Status == "Failed" && s.RetryCount < s.MaxRetries && s.NextRetryAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<SagaStateEntity> AddAsync(SagaEntity sagaEntity, CancellationToken cancellationToken = default)
    {
        var sagaStateEntity = new SagaStateEntity
        {
            Id = Guid.NewGuid(),
            CorrelationId = sagaEntity.CorrelationId,
            Status = sagaEntity.Status,
            SagaType = sagaEntity.SagaType,
            SagaData = sagaEntity.SagaData,
            RetryCount = sagaEntity.RetryCount,
            MaxRetries = sagaEntity.MaxRetries,
            NextRetryAt = sagaEntity.NextRetryAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.SagaStates.Add(sagaStateEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return sagaStateEntity;
    }

    public async Task<bool> UpdateAsync(SagaEntity sagaEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _context.SagaStates.FindAsync(sagaEntity.Id, cancellationToken);
            if (existingEntity == null)
                return false;
                
            existingEntity.Status = sagaEntity.Status;
            existingEntity.SagaData = sagaEntity.SagaData;
            existingEntity.RetryCount = sagaEntity.RetryCount;
            existingEntity.MaxRetries = sagaEntity.MaxRetries;
            existingEntity.NextRetryAt = sagaEntity.NextRetryAt;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
} 