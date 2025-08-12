using System.ComponentModel.DataAnnotations;

namespace AssetService.Application.Features.Asset.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateAssetDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
}

public class UpdateAssetDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
}

public class AssetFilterDto
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
    public int? Page { get; set; }
}

public class AssetListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssetDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime WarrantyExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}