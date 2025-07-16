using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Result<bool>>
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? UserId { get; set; }
} 