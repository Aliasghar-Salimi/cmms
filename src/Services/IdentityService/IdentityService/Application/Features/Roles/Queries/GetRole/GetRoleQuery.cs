using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Queries.GetRole;

public class GetRoleQuery : IRequest<Result<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
} 