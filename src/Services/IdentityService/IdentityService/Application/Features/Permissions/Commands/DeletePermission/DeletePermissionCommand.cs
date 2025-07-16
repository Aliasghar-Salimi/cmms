using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Commands.DeletePermission;

public class DeletePermissionCommand : IRequest<Result<bool>>
{
    public string Id { get; set; } = string.Empty;
} 