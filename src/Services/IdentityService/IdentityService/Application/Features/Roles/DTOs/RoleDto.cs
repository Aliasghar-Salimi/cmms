using System.ComponentModel.DataAnnotations;
using IdentityService.Application.Features.Permissions.DTOs;

namespace IdentityService.Application.Features.Roles.DTOs;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string TenantId { get; set; } = string.Empty;
    
    public List<PermissionDto> Permissions { get; set; } = new();
    
    public int UserCount { get; set; }
    
    public int PermissionCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRoleDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public string TenantId { get; set; } = string.Empty;
    
    public List<string> PermissionIds { get; set; } = new();
}

public class UpdateRoleDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public List<string> PermissionIds { get; set; } = new();
}

public class RoleListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleFilterDto
{
    public string? SearchTerm { get; set; }
    public string? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
} 