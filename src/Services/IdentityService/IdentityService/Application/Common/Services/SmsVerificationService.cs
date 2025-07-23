using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;

namespace IdentityService.Application.Common.Services;

public class SmsVerificationService : ISmsVerificationService
{
    private readonly IdentityServiceDbContext _context;
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsVerificationService> _logger;
    private readonly Random _random;

    public SmsVerificationService(
        IdentityServiceDbContext context,
        ISmsService smsService,
        ILogger<SmsVerificationService> logger)
    {
        _context = context;
        _smsService = smsService;
        _logger = logger;
        _random = new Random();
    }

    public async Task<Result<string>> GenerateAndSendOtpAsync(string phoneNumber, string purpose, Guid? userId = null, int expiryMinutes = 5)
    {
        try
        {
            // Invalidate any existing codes for this phone number and purpose
            await InvalidateOtpAsync(phoneNumber, purpose);

            // Generate a 6-digit OTP
            var otp = GenerateOtp();
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            // Generate MFA token for MFA purposes
            string? mfaToken = null;
            if (purpose == "mfa-login")
            {
                mfaToken = Guid.NewGuid().ToString();
            }

            // Create verification code record
            var verificationCode = new SmsVerificationCode
            {
                PhoneNumber = phoneNumber,
                Code = otp,
                Purpose = purpose,
                ExpiresAt = expiresAt,
                UserId = userId,
                MfaToken = mfaToken,
                IsActive = true
            };

            _context.SmsVerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // Send SMS
            var smsResult = await _smsService.SendOtpAsync(phoneNumber, otp);
            
            if (!smsResult.IsSuccess)
            {
                _logger.LogError("Failed to send OTP SMS to {PhoneNumber}: {Error}", phoneNumber, smsResult.Error);
                return Result<string>.Failure($"Failed to send OTP: {smsResult.Error}");
            }

            _logger.LogInformation("OTP generated and sent successfully to {PhoneNumber} for purpose {Purpose}", phoneNumber, purpose);
            
            // Return MFA token for MFA purposes, otherwise return OTP
            return Result<string>.Success(purpose == "mfa-login" ? mfaToken! : otp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating and sending OTP to {PhoneNumber}", phoneNumber);
            return Result<string>.Failure($"Error generating OTP: {ex.Message}");
        }
    }

    public async Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string code, string purpose)
    {
        try
        {
            var verificationCode = await _context.SmsVerificationCodes
                .Where(vc => vc.PhoneNumber == phoneNumber && 
                            vc.Purpose == purpose && 
                            vc.IsActive)
                .OrderByDescending(vc => vc.CreatedAt)
                .FirstOrDefaultAsync();

            if (verificationCode == null)
            {
                _logger.LogWarning("No verification code found for {PhoneNumber} with purpose {Purpose}", phoneNumber, purpose);
                return Result<bool>.Failure("Invalid or expired verification code");
            }

            // Check if code is expired
            if (verificationCode.IsExpired)
            {
                _logger.LogWarning("Verification code expired for {PhoneNumber}", phoneNumber);
                return Result<bool>.Failure("Verification code has expired");
            }

            // Check if code is already used
            if (verificationCode.IsUsed)
            {
                _logger.LogWarning("Verification code already used for {PhoneNumber}", phoneNumber);
                return Result<bool>.Failure("Verification code has already been used");
            }

            // Check attempts
            if (verificationCode.Attempts >= verificationCode.MaxAttempts)
            {
                _logger.LogWarning("Too many attempts for verification code {PhoneNumber}", phoneNumber);
                return Result<bool>.Failure("Too many verification attempts. Please request a new code.");
            }

            // Increment attempts
            verificationCode.Attempts++;

            // Verify the code
            if (verificationCode.Code != code)
            {
                await _context.SaveChangesAsync();
                _logger.LogWarning("Invalid verification code for {PhoneNumber}", phoneNumber);
                return Result<bool>.Failure("Invalid verification code");
            }

            // Mark as used
            verificationCode.IsUsed = true;
            verificationCode.UsedAt = DateTime.UtcNow;
            verificationCode.IsActive = false;

            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP verified successfully for {PhoneNumber}", phoneNumber);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Error verifying OTP: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ResendOtpAsync(string phoneNumber, string purpose, Guid? userId = null)
    {
        try
        {
            // Check if there's a recent code (within last 1 minute)
            var recentCode = await _context.SmsVerificationCodes
                .Where(vc => vc.PhoneNumber == phoneNumber && 
                            vc.Purpose == purpose && 
                            vc.IsActive &&
                            vc.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
                .FirstOrDefaultAsync();

            if (recentCode != null)
            {
                return Result<bool>.Failure("Please wait at least 1 minute before requesting a new code");
            }

            // Generate and send new OTP
            var result = await GenerateAndSendOtpAsync(phoneNumber, purpose, userId);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("OTP resent successfully to {PhoneNumber}", phoneNumber);
                return Result<bool>.Success(true);
            }
            
            return Result<bool>.Failure(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Error resending OTP: {ex.Message}");
        }
    }

    public async Task<Result<bool>> InvalidateOtpAsync(string phoneNumber, string purpose)
    {
        try
        {
            var activeCodes = await _context.SmsVerificationCodes
                .Where(vc => vc.PhoneNumber == phoneNumber && 
                            vc.Purpose == purpose && 
                            vc.IsActive)
                .ToListAsync();

            foreach (var code in activeCodes)
            {
                code.IsActive = false;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Invalidated {Count} active codes for {PhoneNumber}", activeCodes.Count, phoneNumber);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating OTP for {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Error invalidating OTP: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CleanupExpiredCodesAsync()
    {
        try
        {
            var expiredCodes = await _context.SmsVerificationCodes
                .Where(vc => vc.ExpiresAt < DateTime.UtcNow && vc.IsActive)
                .ToListAsync();

            foreach (var code in expiredCodes)
            {
                code.IsActive = false;
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} expired verification codes", expiredCodes.Count);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired codes");
            return Result<bool>.Failure($"Error cleaning up expired codes: {ex.Message}");
        }
    }

    private string GenerateOtp()
    {
        return _random.Next(100000, 999999).ToString();
    }
} 