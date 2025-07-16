using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Common.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IdentityServiceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionAuthorizationHandler(
        IdentityServiceDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var applicationUser = await _userManager.FindByIdAsync(userId);
        if (applicationUser == null)
        {
            return;
        }

        // Check if user has the required permission
        var hasPermission = await HasPermissionAsync(applicationUser, requirement.Resource, requirement.Action);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> HasPermissionAsync(ApplicationUser user, string resource, string action)
    {
        // Get user's roles
        var userRoles = await _userManager.GetRolesAsync(user);
        if (!userRoles.Any())
        {
            return false;
        }

        // Check if any of the user's roles have the required permission
        var hasPermission = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AnyAsync(rp => 
                userRoles.Contains(rp.Role.Name) &&
                rp.Role.IsActive &&
                rp.Permission.Resource == resource &&
                rp.Permission.Action == action &&
                rp.Permission.IsActive &&
                (rp.Role.TenantId == user.TenantId || rp.Role.TenantId == Guid.Empty));

        return hasPermission;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public string Action { get; }

    public PermissionRequirement(string resource, string action)
    {
        Resource = resource;
        Action = action;
    }
} 