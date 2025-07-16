using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Roles.Queries.GetRole;

public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, Result<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public GetRoleQueryHandler(
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return Result<RoleDto>.Failure("Role not found.");
        }

        // Map to DTO
        var roleDto = _mapper.Map<RoleDto>(role);

        return Result<RoleDto>.Success(roleDto);
    }
} 