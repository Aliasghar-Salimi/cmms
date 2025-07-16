using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleAuthorizationHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
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

        // Check if user has the required role
        var userRoles = await _userManager.GetRolesAsync(applicationUser);
        if (userRoles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }
    }
}

public class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public RoleRequirement(string role)
    {
        Role = role;
    }
} 