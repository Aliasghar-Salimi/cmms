using MediatR;
using AssetService.Application.Features.Asset.DTOs;

namespace AssetService.Application.Features.Asset.Queries.GetAssets;

public class GetAssetsQuery : IRequest<GetAssetsResponse>
{
    public string? Name { get; set; }
    public string? AssetType { get; set; }
    public string? Manufacturer { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public DateTime? WarrantyExpirationDateStart { get; set; }
    public DateTime? WarrantyExpirationDateEnd { get; set; }
    public DateTime? CreatedAtStart { get; set; }
    public DateTime? CreatedAtEnd { get; set; }
    public DateTime? UpdatedAtStart { get; set; }
    public DateTime? UpdatedAtEnd { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}

public class GetAssetsResponse
{
    public List<AssetListDto> Assets { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
} 