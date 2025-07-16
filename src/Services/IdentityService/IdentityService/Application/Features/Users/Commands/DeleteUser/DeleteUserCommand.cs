using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
} 