using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Commands.ToggleRoleStatus;

public class ToggleRoleStatusCommand : IRequest<Result<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
} 