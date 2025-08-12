using MediatR;
using AssetService.Application.Features.Asset.DTOs;

namespace AssetService.Application.Features.Asset.Commands.CreateAsset;

public class CreateAssetCommand : IRequest<AssetDto>
{
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? WarrantyExpirationDate { get; set; }
    public string? UserToken { get; set; }
} 