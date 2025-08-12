using MediatR;
using AssetService.Application.Common.Saga;
using Microsoft.Extensions.Logging;

namespace AssetService.Application.Features.Asset.Commands.DeleteAsset;

public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
{
    private readonly ISagaOrchestrator _sagaOrchestrator;
    private readonly ILogger<DeleteAssetCommandHandler> _logger;

    public DeleteAssetCommandHandler(
        ISagaOrchestrator sagaOrchestrator,
        ILogger<DeleteAssetCommandHandler> logger)
    {
        _sagaOrchestrator = sagaOrchestrator;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteAssetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteAsset command for asset: {AssetId}", request.Id);

        // Extract user token from the command (this would typically come from the HTTP context)
        // For now, we'll use a placeholder - in a real implementation, this would be injected
        var userToken = request.UserToken ?? "placeholder-token";

        var sagaRequest = new DeleteAssetSagaRequest
        {
            AssetId = request.Id,
            UserToken = userToken
        };

        var sagaResult = await _sagaOrchestrator.ExecuteDeleteAssetSagaAsync(sagaRequest, cancellationToken);

        if (!sagaResult.IsSuccess)
        {
            _logger.LogError("DeleteAsset saga failed: {Message}", sagaResult.Message);
            throw new InvalidOperationException($"Failed to delete asset: {sagaResult.Message}");
        }

        _logger.LogInformation("DeleteAsset saga completed successfully. SagaId: {SagaId}", sagaResult.SagaId);

        return true;
    }
} 