using MediatR;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Application.Common.Saga;
using Microsoft.Extensions.Logging;

namespace AssetService.Application.Features.Asset.Commands.UpdateAsset;

public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetDto>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<UpdateAssetCommandHandler> _logger;

    public UpdateAssetCommandHandler(
        ISagaOrchestrator sagaOrchestrator,
        ILogger<UpdateAssetCommandHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }

    public async Task<AssetDto> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateAsset command for asset: {AssetId}", request.Id);

        // Extract user token from the command (this would typically come from the HTTP context)
        // For now, we'll use a placeholder - in a real implementation, this would be injected
        var userToken = request.UserToken ?? "placeholder-token";

        var sagaRequest = new UpdateAssetSagaRequest
        {
            AssetId = request.Id,
            Name = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate,
            UserToken = userToken
        };

        var sagaResult = await _sagaOrchestrator.ExecuteUpdateAssetSagaAsync(sagaRequest, cancellationToken);

        if (!sagaResult.IsSuccess)
        {
            _logger.LogError("UpdateAsset saga failed: {Message}", sagaResult.Message);
            throw new InvalidOperationException($"Failed to update asset: {sagaResult.Message}");
        }

        _logger.LogInformation("UpdateAsset saga completed successfully. SagaId: {SagaId}", sagaResult.SagaId);

        // Return a placeholder DTO since the actual asset update is handled by the saga
        // In a real implementation, you might want to return the actual updated asset
        return new AssetDto
        {
            Id = request.Id,
            Name = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow, // This would be the actual creation date
            UpdatedAt = DateTime.UtcNow
        };
    }
} 