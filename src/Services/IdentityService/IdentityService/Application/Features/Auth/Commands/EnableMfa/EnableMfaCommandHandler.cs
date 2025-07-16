using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Features.Auth.Commands.EnableMfa;

public class EnableMfaCommandHandler : IRequestHandler<EnableMfaCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly ISmsVerificationService _smsVerificationService;

    public EnableMfaCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _context = context;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<bool>> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.PhoneNumber))
        {
            return Result<bool>.Failure("Phone number is required.");
        }

        if (!IsValidMfaType(request.MfaType))
        {
            return Result<bool>.Failure("Invalid MFA type. Supported types: sms, email, totp");
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

        // Check if MFA is already enabled
        var existingMfa = await _context.UserMfas
            .FirstOrDefaultAsync(um => um.UserId == user.Id && um.IsActive);

        if (existingMfa != null && existingMfa.IsEnabled)
        {
            return Result<bool>.Failure("MFA is already enabled for this user.");
        }

        // Create or update MFA record
        if (existingMfa == null)
        {
            existingMfa = new UserMfa
            {
                UserId = user.Id,
                MfaType = request.MfaType,
                BackupPhoneNumber = request.BackupPhoneNumber,
                BackupEmail = request.BackupEmail,
                IsEnabled = false, // Will be enabled after verification
                IsActive = true
            };
            _context.UserMfas.Add(existingMfa);
        }
        else
        {
            existingMfa.MfaType = request.MfaType;
            existingMfa.BackupPhoneNumber = request.BackupPhoneNumber;
            existingMfa.BackupEmail = request.BackupEmail;
            existingMfa.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send verification code
        var smsResult = await _smsVerificationService.GenerateAndSendOtpAsync(
            request.PhoneNumber,
            "mfa-setup",
            user.Id,
            5); // 5 minutes expiry

        if (!smsResult.IsSuccess)
        {
            return Result<bool>.Failure($"Failed to send verification code: {smsResult.Error}");
        }

        return Result<bool>.Success(true);
    }

    private bool IsValidMfaType(string mfaType)
    {
        return mfaType switch
        {
            "sms" => true,
            "email" => true,
            "totp" => true,
            _ => false
        };
    }
} 