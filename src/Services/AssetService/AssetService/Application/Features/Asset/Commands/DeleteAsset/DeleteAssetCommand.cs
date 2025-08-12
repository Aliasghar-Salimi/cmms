using MediatR;

namespace AssetService.Application.Features.Asset.Commands.DeleteAsset;

public class DeleteAssetCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string? UserToken { get; set; }
} 