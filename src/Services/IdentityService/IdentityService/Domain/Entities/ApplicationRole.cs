namespace IdentityService.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using System;

public class ApplicationRole : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public string Description { get; set; }

    public ICollection<ApplicationUser> Users { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}