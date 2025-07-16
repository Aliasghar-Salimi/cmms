using MediatR;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommand : IRequest<Result<PermissionDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
} 