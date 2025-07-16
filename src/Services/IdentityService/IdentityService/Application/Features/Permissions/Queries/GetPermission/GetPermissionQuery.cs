using MediatR;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Queries.GetPermission;

public class GetPermissionQuery : IRequest<Result<PermissionDto>>
{
    public string Id { get; set; } = string.Empty;
} 