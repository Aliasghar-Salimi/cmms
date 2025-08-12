using MediatR;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Application.Common.Saga;
using Microsoft.Extensions.Logging;

namespace AssetService.Application.Features.Asset.Commands.CreateAsset;

public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<CreateAssetCommandHandler> _logger;

    public CreateAssetCommandHandler(
        ISagaOrchestrator sagaOrchestrator,
        ILogger<CreateAssetCommandHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }

    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateAsset command for asset: {AssetName}", request.Name);

        // Extract user token from the command (this would typically come from the HTTP context)
        // For now, we'll use a placeholder - in a real implementation, this would be injected
        var userToken = request.UserToken ?? "placeholder-token";

        var sagaRequest = new CreateAssetSagaRequest
        {
            Name = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate,
            UserToken = userToken
        };

        var sagaResult = await _sagaOrchestrator.ExecuteCreateAssetSagaAsync(sagaRequest, cancellationToken);

        if (!sagaResult.IsSuccess)
        {
            _logger.LogError("CreateAsset saga failed: {Message}", sagaResult.Message);
            throw new InvalidOperationException($"Failed to create asset: {sagaResult.Message}");
        }

        _logger.LogInformation("CreateAsset saga completed successfully. SagaId: {SagaId}", sagaResult.SagaId);

        // Return a placeholder DTO since the actual asset creation is handled by the saga
        // In a real implementation, you might want to return the actual created asset
        return new AssetDto
        {
            Id = Guid.NewGuid(), // This would be the actual asset ID from the saga
            Name = request.Name,
            AssetType = request.AssetType,
            Manufacturer = request.Manufacturer,
            Location = request.Location,
            Status = request.Status,
            WarrantyExpirationDate = request.WarrantyExpirationDate ?? DateTime.UtcNow.AddYears(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
} 