using MediatR;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<Result<UserDto>>
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
} 