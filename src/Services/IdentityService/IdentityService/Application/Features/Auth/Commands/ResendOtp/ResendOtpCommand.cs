using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommand : IRequest<Result<bool>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // "password-reset", "mfa", "mfa-setup"
    public string? TenantId { get; set; }
} 