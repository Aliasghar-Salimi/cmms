using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest<Result<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> PermissionIds { get; set; } = new();
} 