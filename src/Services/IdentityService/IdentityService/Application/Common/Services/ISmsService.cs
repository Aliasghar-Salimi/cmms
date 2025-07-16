namespace IdentityService.Application.Common.Services;

public interface ISmsService
{
    Task<Result<bool>> SendOtpAsync(string phoneNumber, string otp, string template = "otp");
    Task<Result<bool>> SendPasswordResetAsync(string phoneNumber, string resetCode, string template = "password-reset");
    Task<Result<bool>> SendMfaCodeAsync(string phoneNumber, string mfaCode, string template = "mfa");
    Task<Result<bool>> SendWelcomeMessageAsync(string phoneNumber, string userName, string template = "welcome");
    Task<Result<bool>> SendSecurityAlertAsync(string phoneNumber, string alertMessage, string template = "security-alert");
} 