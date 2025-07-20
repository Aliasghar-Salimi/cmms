using System.ComponentModel.DataAnnotations;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Roles.DTOs;

namespace IdentityService.Application.Features.Tenants.DTOs;

public class TenantDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<UserDto> Users { get; set; } = new();
    
    public List<RoleDto> Roles { get; set; } = new();
    
    public int UserCount { get; set; }
    
    public int RoleCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTenantDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateTenantDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TenantListDto
{
    public List<TenantDto> Tenants { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TenantFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
} 