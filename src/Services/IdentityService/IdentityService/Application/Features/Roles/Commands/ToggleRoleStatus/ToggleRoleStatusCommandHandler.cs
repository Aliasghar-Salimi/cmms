using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Application.Common;
using AutoMapper;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Roles.Commands.ToggleRoleStatus;

public class ToggleRoleStatusCommandHandler : IRequestHandler<ToggleRoleStatusCommand, Result<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public ToggleRoleStatusCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> Handle(ToggleRoleStatusCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return Result<RoleDto>.Failure("Role not found.");
        }

        // Toggle the active status
        role.IsActive = !role.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<RoleDto>.Failure($"Failed to toggle role status: {errors}");
        }

        // Map to DTO
        var roleDto = _mapper.Map<RoleDto>(role);

        return Result<RoleDto>.Success(roleDto);
    }
} 