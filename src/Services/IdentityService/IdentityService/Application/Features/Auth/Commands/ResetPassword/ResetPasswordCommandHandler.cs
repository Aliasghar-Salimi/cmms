using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsVerificationService _smsVerificationService;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.PhoneNumber))
        {
            return Result<bool>.Failure("Phone number is required.");
        }

        if (string.IsNullOrEmpty(request.VerificationCode))
        {
            return Result<bool>.Failure("Verification code is required.");
        }

        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return Result<bool>.Failure("New password is required.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Result<bool>.Failure("New password and confirmation password do not match.");
        }

        // Find user by phone number
        var user = await _userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            return Result<bool>.Failure("Invalid phone number or verification code.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<bool>.Failure("User account is not active.");
        }

        // Verify SMS code
        var verificationResult = await _smsVerificationService.VerifyOtpAsync(
            request.PhoneNumber, 
            request.VerificationCode, 
            "password-reset");

        if (!verificationResult.IsSuccess)
        {
            return Result<bool>.Failure($"Verification failed: {verificationResult.Error}");
        }

        // Reset password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure($"Password reset failed: {errors}");
        }

        // Update user
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result<bool>.Success(true);
    }
} 