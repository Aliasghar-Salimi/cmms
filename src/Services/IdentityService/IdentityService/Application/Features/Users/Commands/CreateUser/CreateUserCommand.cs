using MediatR;
using IdentityService.Application.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid TenantId { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
} 