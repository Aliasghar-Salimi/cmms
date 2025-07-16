using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Features.Auth.Commands.ResendOtp;

public class ResendOtpCommandHandler : IRequestHandler<ResendOtpCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsVerificationService _smsVerificationService;

    public ResendOtpCommandHandler(
        UserManager<ApplicationUser> userManager,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<bool>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.PhoneNumber))
        {
            return Result<bool>.Failure("Phone number is required.");
        }

        if (string.IsNullOrEmpty(request.Purpose))
        {
            return Result<bool>.Failure("Purpose is required.");
        }

        // Find user by phone number
        var user = await _userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            // Don't reveal if user exists or not for security reasons
            return Result<bool>.Success(true);
        }

        // Validate tenant if provided
        if (!string.IsNullOrEmpty(request.TenantId) && user.TenantId != Guid.Parse(request.TenantId))
        {
            return Result<bool>.Success(true); // Don't reveal user existence
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<bool>.Success(true); // Don't reveal user existence
        }

        // Resend OTP
        var result = await _smsVerificationService.ResendOtpAsync(
            request.PhoneNumber,
            request.Purpose,
            user.Id);

        if (!result.IsSuccess)
        {
            return Result<bool>.Failure($"Failed to resend verification code: {result.Error}");
        }

        return Result<bool>.Success(true);
    }
} 