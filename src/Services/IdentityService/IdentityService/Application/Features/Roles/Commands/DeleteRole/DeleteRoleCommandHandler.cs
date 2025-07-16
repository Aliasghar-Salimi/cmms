using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return Result<bool>.Failure("Role not found.");
        }

        // Check if role has any users assigned
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        if (usersInRole.Any())
        {
            return Result<bool>.Failure($"Cannot delete role '{role.Name}' because it has {usersInRole.Count} users assigned.");
        }

        // Check if it's a system role (prevent deletion of critical roles)
        if (role.Name.Equals("SystemAdmin", StringComparison.OrdinalIgnoreCase) ||
            role.Name.Equals("TenantAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure($"Cannot delete system role '{role.Name}'.");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure($"Failed to delete role: {errors}");
        }

        return Result<bool>.Success(true);
    }
} 