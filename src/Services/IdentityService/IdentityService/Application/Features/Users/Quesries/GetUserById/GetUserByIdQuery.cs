using MediatR;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Users.Quesries.GetUserById;

public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid Id { get; set; }
} 