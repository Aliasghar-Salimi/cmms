using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Features.Permissions.DTOs;

public class PermissionDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public string TenantId { get; set; } = string.Empty;
    
    public string PermissionKey => $"{Resource}.{Action}";
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePermissionDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
    
    public string TenantId { get; set; } = string.Empty;
}

public class UpdatePermissionDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;
}

public class PermissionListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int RoleCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionFilterDto
{
    public string? SearchTerm { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public string? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}

public class PermissionTemplateDto
{
    public string Resource { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
    public string? Description { get; set; }
}

public class AutoGeneratePermissionsDto
{
    public string TenantId { get; set; } = string.Empty;
    public List<PermissionTemplateDto> Templates { get; set; } = new();
    public bool OverwriteExisting { get; set; } = false;
} 