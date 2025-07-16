using MediatR;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<MfaLoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantId { get; set; }
} 