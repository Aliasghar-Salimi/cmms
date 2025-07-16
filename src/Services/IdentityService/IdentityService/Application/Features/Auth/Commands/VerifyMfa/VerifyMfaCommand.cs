using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.VerifyMfa;

public class VerifyMfaCommand : IRequest<Result<bool>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string Purpose { get; set; } = "mfa"; // "mfa", "mfa-setup"
} 