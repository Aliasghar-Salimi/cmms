using MediatR;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
} 