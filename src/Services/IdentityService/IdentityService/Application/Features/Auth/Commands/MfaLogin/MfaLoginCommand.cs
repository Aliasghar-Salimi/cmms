using MediatR;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.MfaLogin;

public class MfaLoginCommand : IRequest<Result<LoginResponseDto>>
{
    public string MfaToken { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
} 