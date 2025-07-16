using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Commands.DeletePermission;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeletePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions.FindAsync(request.Id);
        if (permission == null)
        {
            return Result<bool>.Failure("Permission not found.");
        }

        // Check if permission is assigned to any roles
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.PermissionId == Guid.Parse(request.Id))
            .CountAsync(cancellationToken);

        if (rolePermissions > 0)
        {
            return Result<bool>.Failure($"Cannot delete permission '{permission.Name}' because it is assigned to {rolePermissions} roles.");
        }

        // Check if it's a system permission (prevent deletion of critical permissions)
        if (permission.Resource.Equals("System", StringComparison.OrdinalIgnoreCase) ||
            permission.Resource.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure($"Cannot delete system permission '{permission.Name}'.");
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
} 