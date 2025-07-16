using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public UpdateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return Result<RoleDto>.Failure("Role not found.");
        }

        // Check if the new name conflicts with existing role in the same tenant
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null && existingRole.Id != Guid.Parse(request.Id) && existingRole.TenantId == role.TenantId)
        {
            return Result<RoleDto>.Failure($"Role '{request.Name}' already exists in this tenant.");
        }

        // Update role properties
        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<RoleDto>.Failure($"Failed to update role: {errors}");
        }

        // Update role permissions
        if (request.PermissionIds != null)
        {
            // Remove existing permissions
            var existingRolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .ToListAsync(cancellationToken);
            
            _context.RolePermissions.RemoveRange(existingRolePermissions);

            // Add new permissions
            if (request.PermissionIds.Any())
            {
                var newRolePermissions = request.PermissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = Guid.Parse(permissionId)
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(newRolePermissions, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Map to DTO
        var roleDto = _mapper.Map<RoleDto>(role);

        return Result<RoleDto>.Success(roleDto);
    }
} 