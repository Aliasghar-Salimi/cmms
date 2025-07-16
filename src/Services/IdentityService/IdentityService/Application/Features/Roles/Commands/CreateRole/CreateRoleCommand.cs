using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<Result<RoleDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public List<string> PermissionIds { get; set; } = new();
} 