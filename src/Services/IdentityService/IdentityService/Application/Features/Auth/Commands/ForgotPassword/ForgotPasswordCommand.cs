using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result<bool>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? TenantId { get; set; }
} 