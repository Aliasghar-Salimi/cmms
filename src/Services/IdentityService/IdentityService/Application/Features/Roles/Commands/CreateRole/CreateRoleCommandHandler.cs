using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role already exists in the tenant
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null && existingRole.TenantId == Guid.Parse(request.TenantId))
        {
            return Result<RoleDto>.Failure($"Role '{request.Name}' already exists in this tenant.");
        }

        // Create new role
        var role = new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description,
            TenantId = Guid.Parse(request.TenantId),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<RoleDto>.Failure($"Failed to create role: {errors}");
        }

        // Assign permissions to role
        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            var rolePermissions = request.PermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = Guid.Parse(permissionId)
            }).ToList();

            await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Map to DTO
        var roleDto = _mapper.Map<RoleDto>(role);

        return Result<RoleDto>.Success(roleDto);
    }
} 