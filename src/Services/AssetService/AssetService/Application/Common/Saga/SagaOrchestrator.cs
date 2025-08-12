using Microsoft.Extensions.Logging;
using AssetService.Application.Common.Services;
using AssetService.Application.Common.SharedModels;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Domain.Entities;
using AssetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Text.Json;

namespace AssetService.Application.Common.Saga;

public class SagaOrchestrator : ISagaOrchestrator
{
    private readonly ILogger<SagaOrchestrator> _logger;
    private readonly IIdentityServiceClient _identityServiceClient;
    private readonly IEventPublisherService _eventPublisher;
    private readonly AssetServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISagaStateRepository _sagaStateRepository;

    public SagaOrchestrator(
        ILogger<SagaOrchestrator> logger,
        IIdentityServiceClient identityServiceClient,
        IEventPublisherService eventPublisher,
        AssetServiceDbContext context,
        IMapper mapper,
        ISagaStateRepository sagaStateRepository)
    {
        _logger = logger;
        _identityServiceClient = identityServiceClient;
        _eventPublisher = eventPublisher;
        _context = context;
        _mapper = mapper;
        _sagaStateRepository = sagaStateRepository;
    }

    public async Task<SagaResult> ExecuteCreateAssetSagaAsync(CreateAssetSagaRequest request, CancellationToken cancellationToken = default)
    {
        var sagaId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var sagaResult = new SagaResult
        {
            SagaId = sagaId,
            Steps = new List<SagaStepResult>()
        };

        _logger.LogInformation("Starting CreateAsset saga {SagaId} with correlation {CorrelationId}", sagaId, correlationId);

        // Create and persist saga state
        var sagaState = new AssetCreationSagaState
        {
            SagaId = Guid.Parse(sagaId),
            CorrelationId = correlationId,
            AssetName = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1)
        };

        var sagaStateEntity = await PersistSagaStateAsync(sagaState, "AssetCreation", cancellationToken);

        try
        {
            // Step 1: Validate user permissions
            var permissionStep = await ExecutePermissionValidationStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(permissionStep);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "InProgress", cancellationToken);

            if (!permissionStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Permission validation failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.UserValidated = true;
            await PersistSagaStateAsync(sagaState, "AssetCreation", cancellationToken);

            // Step 2: Create asset
            var createAssetStep = await ExecuteCreateAssetStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(createAssetStep);

            if (!createAssetStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Asset creation failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.AssetCreated = true;
            await PersistSagaStateAsync(sagaState, "AssetCreation", cancellationToken);

            // Step 3: Publish asset created event
            var publishEventStep = await ExecutePublishEventStep("AssetCreated", sagaId, cancellationToken);
            sagaResult.Steps.Add(publishEventStep);

            if (!publishEventStep.IsSuccess)
            {
                // Try to compensate for the asset creation
                await CompensateAssetCreationAsync(sagaState, cancellationToken);
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Event publishing failed, asset creation compensated";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.EventPublished = true;
            await PersistSagaStateAsync(sagaState, "AssetCreation", cancellationToken);

            await UpdateSagaStateAsync(sagaStateEntity.Id, "Completed", cancellationToken);
            sagaResult.IsSuccess = true;
            sagaResult.Message = "Asset created successfully";
            sagaResult.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("CreateAsset saga {SagaId} completed successfully", sagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsset saga {SagaId} failed", sagaId);
            
            // Attempt compensation
            await CompensateAssetCreationAsync(sagaState, cancellationToken);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
            
            sagaResult.IsSuccess = false;
            sagaResult.Message = $"Saga failed: {ex.Message}";
            sagaResult.CompletedAt = DateTime.UtcNow;
        }

        return sagaResult;
    }

    public async Task<SagaResult> ExecuteUpdateAssetSagaAsync(UpdateAssetSagaRequest request, CancellationToken cancellationToken = default)
    {
        var sagaId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var sagaResult = new SagaResult
        {
            SagaId = sagaId,
            Steps = new List<SagaStepResult>()
        };

        _logger.LogInformation("Starting UpdateAsset saga {SagaId} with correlation {CorrelationId}", sagaId, correlationId);

        // Create and persist saga state
        var sagaState = new AssetUpdateSagaState
        {
            SagaId = Guid.Parse(sagaId),
            CorrelationId = correlationId,
            AssetId = request.AssetId,
            AssetName = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1)
        };

        var sagaStateEntity = await PersistSagaStateAsync(sagaState, "AssetUpdate", cancellationToken);

        try
        {
            // Step 1: Validate user permissions
            var permissionStep = await ExecutePermissionValidationStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(permissionStep);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "InProgress", cancellationToken);

            if (!permissionStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Permission validation failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.UserValidated = true;
            await PersistSagaStateAsync(sagaState, "AssetUpdate", cancellationToken);

            // Step 2: Update asset
            var updateAssetStep = await ExecuteUpdateAssetStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(updateAssetStep);

            if (!updateAssetStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Asset update failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.AssetUpdated = true;
            await PersistSagaStateAsync(sagaState, "AssetUpdate", cancellationToken);

            // Step 3: Publish asset updated event
            var publishEventStep = await ExecutePublishEventStep("AssetUpdated", sagaId, cancellationToken);
            sagaResult.Steps.Add(publishEventStep);

            if (!publishEventStep.IsSuccess)
            {
                // Try to compensate for the asset update
                await CompensateAssetUpdateAsync(sagaState, cancellationToken);
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Event publishing failed, asset update compensated";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.EventPublished = true;
            await PersistSagaStateAsync(sagaState, "AssetUpdate", cancellationToken);

            await UpdateSagaStateAsync(sagaStateEntity.Id, "Completed", cancellationToken);
            sagaResult.IsSuccess = true;
            sagaResult.Message = "Asset updated successfully";
            sagaResult.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("UpdateAsset saga {SagaId} completed successfully", sagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsset saga {SagaId} failed", sagaId);
            
            // Attempt compensation
            await CompensateAssetUpdateAsync(sagaState, cancellationToken);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
            
            sagaResult.IsSuccess = false;
            sagaResult.Message = $"Saga failed: {ex.Message}";
            sagaResult.CompletedAt = DateTime.UtcNow;
        }

        return sagaResult;
    }

    public async Task<SagaResult> ExecuteDeleteAssetSagaAsync(DeleteAssetSagaRequest request, CancellationToken cancellationToken = default)
    {
        var sagaId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var sagaResult = new SagaResult
        {
            SagaId = sagaId,
            Steps = new List<SagaStepResult>()
        };

        _logger.LogInformation("Starting DeleteAsset saga {SagaId} with correlation {CorrelationId}", sagaId, correlationId);

        // Create and persist saga state
        var sagaState = new AssetDeletionSagaState
        {
            SagaId = Guid.Parse(sagaId),
            CorrelationId = correlationId,
            AssetId = request.AssetId
        };

        var sagaStateEntity = await PersistSagaStateAsync(sagaState, "AssetDeletion", cancellationToken);

        try
        {
            // Step 1: Validate user permissions
            var permissionStep = await ExecutePermissionValidationStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(permissionStep);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "InProgress", cancellationToken);

            if (!permissionStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Permission validation failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.UserValidated = true;
            await PersistSagaStateAsync(sagaState, "AssetDeletion", cancellationToken);

            // Step 2: Delete asset
            var deleteAssetStep = await ExecuteDeleteAssetStep(request, sagaId, cancellationToken);
            sagaResult.Steps.Add(deleteAssetStep);

            if (!deleteAssetStep.IsSuccess)
            {
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Asset deletion failed";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.AssetDeleted = true;
            await PersistSagaStateAsync(sagaState, "AssetDeletion", cancellationToken);

            // Step 3: Publish asset deleted event
            var publishEventStep = await ExecutePublishEventStep("AssetDeleted", sagaId, cancellationToken);
            sagaResult.Steps.Add(publishEventStep);

            if (!publishEventStep.IsSuccess)
            {
                // Try to compensate for the asset deletion
                await CompensateAssetDeletionAsync(sagaState, cancellationToken);
                await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
                sagaResult.IsSuccess = false;
                sagaResult.Message = "Event publishing failed, asset deletion compensated";
                sagaResult.CompletedAt = DateTime.UtcNow;
                return sagaResult;
            }

            sagaState.EventPublished = true;
            await PersistSagaStateAsync(sagaState, "AssetDeletion", cancellationToken);

            await UpdateSagaStateAsync(sagaStateEntity.Id, "Completed", cancellationToken);
            sagaResult.IsSuccess = true;
            sagaResult.Message = "Asset deleted successfully";
            sagaResult.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("DeleteAsset saga {SagaId} completed successfully", sagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsset saga {SagaId} failed", sagaId);
            
            // Attempt compensation
            await CompensateAssetDeletionAsync(sagaState, cancellationToken);
            await UpdateSagaStateAsync(sagaStateEntity.Id, "Failed", cancellationToken);
            
            sagaResult.IsSuccess = false;
            sagaResult.Message = $"Saga failed: {ex.Message}";
            sagaResult.CompletedAt = DateTime.UtcNow;
        }

        return sagaResult;
    }

    public async Task<SagaResult> CompensateAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting compensation for saga {SagaId}", sagaId);
        
        try
        {
            var sagaEntity = await _sagaStateRepository.GetByIdAsync(Guid.Parse(sagaId), cancellationToken);
            if (sagaEntity == null)
            {
                return new SagaResult
                {
                    SagaId = sagaId,
                    IsSuccess = false,
                    Message = "Saga not found",
                    CompletedAt = DateTime.UtcNow
                };
            }

            // Deserialize saga state
            var sagaState = JsonSerializer.Deserialize<BaseSagaState>(sagaEntity.SagaData);
            if (sagaState == null)
            {
                return new SagaResult
                {
                    SagaId = sagaId,
                    IsSuccess = false,
                    Message = "Failed to deserialize saga state",
                    CompletedAt = DateTime.UtcNow
                };
            }

            // Perform compensation based on saga type
            bool compensationSuccess = false;
            switch (sagaEntity.SagaType)
            {
                case "AssetCreation":
                    if (sagaState is AssetCreationSagaState creationState)
                    {
                        compensationSuccess = await CompensateAssetCreationAsync(creationState, cancellationToken);
                    }
                    break;
                case "AssetUpdate":
                    if (sagaState is AssetUpdateSagaState updateState)
                    {
                        compensationSuccess = await CompensateAssetUpdateAsync(updateState, cancellationToken);
                    }
                    break;
                case "AssetDeletion":
                    if (sagaState is AssetDeletionSagaState deletionState)
                    {
                        compensationSuccess = await CompensateAssetDeletionAsync(deletionState, cancellationToken);
                    }
                    break;
            }

            if (compensationSuccess)
            {
                await _sagaStateRepository.UpdateStatusAsync(Guid.Parse(sagaId), "Compensated", cancellationToken);
                return new SagaResult
                {
                    SagaId = sagaId,
                    IsSuccess = true,
                    Message = "Compensation completed successfully",
                    CompletedAt = DateTime.UtcNow
                };
            }
            else
            {
                return new SagaResult
                {
                    SagaId = sagaId,
                    IsSuccess = false,
                    Message = "Compensation failed",
                    CompletedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation failed for saga {SagaId}", sagaId);
            return new SagaResult
            {
                SagaId = sagaId,
                IsSuccess = false,
                Message = $"Compensation failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStepResult> ExecutePermissionValidationStep(object request, string sagaId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing permission validation step for saga {SagaId}", sagaId);
            
            // Simulate permission validation - in real implementation, this would call the identity service
            await Task.Delay(100, cancellationToken); // Simulate async operation
            
            var stepResult = new SagaStepResult
            {
                StepName = "PermissionValidation",
                IsSuccess = true,
                Message = "Permission validation successful",
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Permission validation step completed for saga {SagaId}", sagaId);
            return stepResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission validation step failed for saga {SagaId}", sagaId);
            return new SagaStepResult
            {
                StepName = "PermissionValidation",
                IsSuccess = false,
                Message = $"Permission validation failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStepResult> ExecuteCreateAssetStep(CreateAssetSagaRequest request, string sagaId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing create asset step for saga {SagaId}", sagaId);
            
            var asset = new Asset
            {
                Name = request.Name,
                AssetType = request.AssetType,
                Manufacturer = request.Manufacturer,
                Location = request.Location,
                Status = request.Status,
                WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);
            
            var stepResult = new SagaStepResult
            {
                StepName = "CreateAsset",
                IsSuccess = true,
                Message = $"Asset created with ID: {asset.Id}",
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Create asset step completed for saga {SagaId}, asset ID: {AssetId}", sagaId, asset.Id);
            return stepResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create asset step failed for saga {SagaId}", sagaId);
            return new SagaStepResult
            {
                StepName = "CreateAsset",
                IsSuccess = false,
                Message = $"Asset creation failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStepResult> ExecuteUpdateAssetStep(UpdateAssetSagaRequest request, string sagaId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing update asset step for saga {SagaId}", sagaId);
            
            var asset = await _context.Assets.FindAsync(request.AssetId, cancellationToken);
            if (asset == null)
            {
                return new SagaStepResult
                {
                    StepName = "UpdateAsset",
                    IsSuccess = false,
                    Message = $"Asset with ID {request.AssetId} not found",
                    CompletedAt = DateTime.UtcNow
                };
            }
            
            // Store original values for compensation
            var originalName = asset.Name;
            var originalAssetType = asset.AssetType;
            var originalManufacturer = asset.Manufacturer;
            var originalLocation = asset.Location;
            var originalStatus = asset.Status;
            var originalWarrantyExpirationDate = asset.WarrantyExpirationDate;
            
            // Update asset properties
            asset.Name = request.Name;
            asset.AssetType = request.AssetType;
            asset.Manufacturer = request.Manufacturer;
            asset.Location = request.Location;
            asset.Status = request.Status;
            asset.WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1);
            asset.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            var stepResult = new SagaStepResult
            {
                StepName = "UpdateAsset",
                IsSuccess = true,
                Message = $"Asset updated successfully",
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Update asset step completed for saga {SagaId}, asset ID: {AssetId}", sagaId, asset.Id);
            return stepResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update asset step failed for saga {SagaId}", sagaId);
            return new SagaStepResult
            {
                StepName = "UpdateAsset",
                IsSuccess = false,
                Message = $"Asset update failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStepResult> ExecuteDeleteAssetStep(DeleteAssetSagaRequest request, string sagaId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing delete asset step for saga {SagaId}", sagaId);
            
            var asset = await _context.Assets.FindAsync(request.AssetId, cancellationToken);
            if (asset == null)
            {
                return new SagaStepResult
                {
                    StepName = "DeleteAsset",
                    IsSuccess = false,
                    Message = $"Asset with ID {request.AssetId} not found",
                    CompletedAt = DateTime.UtcNow
                };
            }
            
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync(cancellationToken);
            
            var stepResult = new SagaStepResult
            {
                StepName = "DeleteAsset",
                IsSuccess = true,
                Message = $"Asset deleted successfully",
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Delete asset step completed for saga {SagaId}, asset ID: {AssetId}", sagaId, asset.Id);
            return stepResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete asset step failed for saga {SagaId}", sagaId);
            return new SagaStepResult
            {
                StepName = "DeleteAsset",
                IsSuccess = false,
                Message = $"Asset deletion failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStepResult> ExecutePublishEventStep(string eventType, string sagaId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing publish event step for saga {SagaId}, event type: {EventType}", sagaId, eventType);
            
            // Simulate event publishing - in real implementation, this would call the event publisher service
            await Task.Delay(100, cancellationToken); // Simulate async operation
            
            var stepResult = new SagaStepResult
            {
                StepName = "PublishEvent",
                IsSuccess = true,
                Message = $"Event {eventType} published successfully",
                CompletedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Publish event step completed for saga {SagaId}, event type: {EventType}", sagaId, eventType);
            return stepResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish event step failed for saga {SagaId}, event type: {EventType}", sagaId, eventType);
            return new SagaStepResult
            {
                StepName = "PublishEvent",
                IsSuccess = false,
                Message = $"Event publishing failed: {ex.Message}",
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    private async Task<SagaStateEntity> PersistSagaStateAsync(BaseSagaState sagaState, string sagaType, CancellationToken cancellationToken)
    {
        var sagaEntity = new SagaEntity
        {
            SagaId = sagaState.SagaId,
            CorrelationId = sagaState.CorrelationId,
            SagaType = sagaType,
            SagaData = JsonSerializer.Serialize(sagaState),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var sagaStateEntity = await _sagaStateRepository.AddAsync(sagaEntity, cancellationToken);
        
        return sagaStateEntity;
    }

    private async Task UpdateSagaStateAsync(Guid sagaId, string status, CancellationToken cancellationToken)
    {
        await _sagaStateRepository.UpdateStatusAsync(sagaId, status, cancellationToken);
    }

    private async Task<bool> CompensateAssetCreationAsync(AssetCreationSagaState sagaState, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Compensating AssetCreation saga {SagaId}", sagaState.SagaId);
        try
        {
            var asset = await _context.Assets.FindAsync(sagaState.AssetId, cancellationToken);
            if (asset != null)
            {
                _context.Assets.Remove(asset);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Asset with ID {AssetId} deleted successfully during compensation", sagaState.AssetId);
                return true;
            }
            _logger.LogWarning("Asset with ID {AssetId} not found for compensation", sagaState.AssetId);
            return true; // No asset to delete, but no error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation for AssetCreation saga {SagaId} failed", sagaState.SagaId);
            return false;
        }
    }

    private async Task<bool> CompensateAssetUpdateAsync(AssetUpdateSagaState sagaState, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Compensating AssetUpdate saga {SagaId}", sagaState.SagaId);
        try
        {
            var asset = await _context.Assets.FindAsync(sagaState.AssetId, cancellationToken);
            if (asset != null)
            {
                // Revert changes made to the asset
                asset.Name = sagaState.OriginalAssetName;
                asset.AssetType = sagaState.OriginalAssetType;
                asset.Manufacturer = sagaState.OriginalManufacturer;
                asset.Location = sagaState.OriginalLocation;
                asset.Status = sagaState.OriginalStatus;
                asset.WarrantyExpirationDate = sagaState.OriginalWarrantyExpirationDate;
                asset.UpdatedAt = DateTime.UtcNow; // Ensure updated timestamp is correct
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Asset with ID {AssetId} updated back successfully during compensation", sagaState.AssetId);
                return true;
            }
            _logger.LogWarning("Asset with ID {AssetId} not found for compensation", sagaState.AssetId);
            return true; // No asset to update, but no error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation for AssetUpdate saga {SagaId} failed", sagaState.SagaId);
            return false;
        }
    }

    private async Task<bool> CompensateAssetDeletionAsync(AssetDeletionSagaState sagaState, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Compensating AssetDeletion saga {SagaId}", sagaState.SagaId);
        try
        {
            var asset = await _context.Assets.FindAsync(sagaState.AssetId, cancellationToken);
            if (asset != null)
            {
                _context.Assets.Add(asset); // Re-add the asset
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Asset with ID {AssetId} re-added successfully during compensation", sagaState.AssetId);
                return true;
            }
            _logger.LogWarning("Asset with ID {AssetId} not found for compensation", sagaState.AssetId);
            return true; // No asset to re-add, but no error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compensation for AssetDeletion saga {SagaId} failed", sagaState.SagaId);
            return false;
        }
    }
} 