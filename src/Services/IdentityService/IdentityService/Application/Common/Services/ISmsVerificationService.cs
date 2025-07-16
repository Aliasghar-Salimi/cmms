namespace IdentityService.Application.Common.Services;

public interface ISmsVerificationService
{
    Task<Result<string>> GenerateAndSendOtpAsync(string phoneNumber, string purpose, Guid? userId = null, int expiryMinutes = 5);
    Task<Result<bool>> VerifyOtpAsync(string phoneNumber, string code, string purpose);
    Task<Result<bool>> ResendOtpAsync(string phoneNumber, string purpose, Guid? userId = null);
    Task<Result<bool>> InvalidateOtpAsync(string phoneNumber, string purpose);
    Task<Result<bool>> CleanupExpiredCodesAsync();
} 