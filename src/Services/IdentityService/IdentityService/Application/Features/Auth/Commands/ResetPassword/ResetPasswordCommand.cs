using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<Result<bool>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
} 