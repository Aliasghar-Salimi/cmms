using MediatR;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.Login;

// GUIDANCE: IRequest<Result<MfaLoginResponseDto>> is from MediatR, a library for handling requests in a clean way.
// IRequest<T> means this class is a request that expects a response of type T.
// Result<MfaLoginResponseDto> means the response will be a Result object (usually for success/failure info) containing an MfaLoginResponseDto (the login data).
// So, LoginCommand is a request that, when handled, returns a result with login info.

public class LoginCommand : IRequest<Result<MfaLoginResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TenantId { get; set; }
} 