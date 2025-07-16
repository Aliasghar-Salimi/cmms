using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest<Result<bool>>
{
    public string Id { get; set; } = string.Empty;
} 