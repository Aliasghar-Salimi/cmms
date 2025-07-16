using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsVerificationService _smsVerificationService;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.PhoneNumber))
        {
            return Result<bool>.Failure("Phone number is required.");
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

        // Generate and send SMS verification code
        var smsResult = await _smsVerificationService.GenerateAndSendOtpAsync(
            request.PhoneNumber, 
            "password-reset", 
            user.Id, 
            10); // 10 minutes expiry for password reset
        
        if (!smsResult.IsSuccess)
        {
            return Result<bool>.Failure($"Failed to send verification code: {smsResult.Error}");
        }
        
        return Result<bool>.Success(true);
    }
} 