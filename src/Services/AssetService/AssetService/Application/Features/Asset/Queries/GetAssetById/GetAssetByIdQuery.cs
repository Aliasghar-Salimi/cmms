using MediatR;
using AssetService.Application.Features.Asset.DTOs;

namespace AssetService.Application.Features.Asset.Queries.GetAssetById;

public class GetAssetByIdQuery : IRequest<AssetDto?>
{
    public Guid Id { get; set; }
} 