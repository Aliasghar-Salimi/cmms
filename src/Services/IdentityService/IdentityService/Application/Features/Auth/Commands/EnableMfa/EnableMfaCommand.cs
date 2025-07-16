using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.EnableMfa;

public class EnableMfaCommand : IRequest<Result<bool>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string MfaType { get; set; } = "sms"; // "sms", "email", "totp"
    public string? BackupPhoneNumber { get; set; }
    public string? BackupEmail { get; set; }
} 