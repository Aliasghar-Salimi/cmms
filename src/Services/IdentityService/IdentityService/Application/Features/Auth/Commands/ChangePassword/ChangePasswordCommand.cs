using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<Result<bool>>
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
} 