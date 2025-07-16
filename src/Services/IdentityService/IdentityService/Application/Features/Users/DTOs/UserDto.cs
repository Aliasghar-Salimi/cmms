using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Features.Users.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string TenantId { get; set; } = string.Empty;
    
    public List<string> Roles { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public string TenantId { get; set; } = string.Empty;
    
    public List<string> RoleNames { get; set; } = new();
}

public class UpdateUserDto
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    public List<string> RoleNames { get; set; } = new();
}

public class UserListDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int RoleCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserFilterDto
{
    public string? SearchTerm { get; set; }
    public string? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "UserName";
    public bool SortDescending { get; set; } = false;
} 