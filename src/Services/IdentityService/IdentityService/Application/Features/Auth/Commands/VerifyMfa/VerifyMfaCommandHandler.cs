using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Features.Auth.Commands.VerifyMfa;

public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly ISmsVerificationService _smsVerificationService;

    public VerifyMfaCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _context = context;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<bool>> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
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

        // Find user by phone number
        var user = await _userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            return Result<bool>.Failure("User not found.");
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
            request.Purpose);

        if (!verificationResult.IsSuccess)
        {
            return Result<bool>.Failure($"Verification failed: {verificationResult.Error}");
        }

        // If this is for MFA setup, enable MFA
        if (request.Purpose == "mfa-setup")
        {
            var userMfa = await _context.UserMfas
                .FirstOrDefaultAsync(um => um.UserId == user.Id && um.IsActive);

            if (userMfa != null)
            {
                userMfa.IsEnabled = true;
                userMfa.LastUsedAt = DateTime.UtcNow;
                userMfa.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return Result<bool>.Success(true);
    }
} 