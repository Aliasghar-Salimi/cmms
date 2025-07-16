using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IdentityService.Application.Common;
using System.Text.Json;

namespace IdentityService.Application.Common.Services;

public class KavenegarSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KavenegarSmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.kavenegar.com/v1";

    public KavenegarSmsService(
        IConfiguration configuration, 
        ILogger<KavenegarSmsService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
        
        _apiKey = _configuration["Kavenegar:ApiKey"] ?? "5734736879526A37446F6656703231373769567158576653476D484D6F68337046727563352B684C52486F3D";
    }

    public async Task<Result<bool>> SendOtpAsync(string phoneNumber, string otp, string template = "otp")
    {
        try
        {
            var message = $"کد تایید شما: {otp}\n\nCMMS Identity Service\n\nاین کد تا 5 دقیقه معتبر است.";
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("OTP SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Failed to send OTP SMS: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SendPasswordResetAsync(string phoneNumber, string resetCode, string template = "password-reset")
    {
        try
        {
            var message = $"کد بازنشانی رمز عبور: {resetCode}\n\nCMMS Identity Service\n\nاین کد تا 10 دقیقه معتبر است.";
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Password reset SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Failed to send password reset SMS: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SendMfaCodeAsync(string phoneNumber, string mfaCode, string template = "mfa")
    {
        try
        {
            var message = $"کد احراز هویت دو مرحله‌ای: {mfaCode}\n\nCMMS Identity Service\n\nاین کد تا 3 دقیقه معتبر است.";
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("MFA SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MFA SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Failed to send MFA SMS: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SendWelcomeMessageAsync(string phoneNumber, string userName, string template = "welcome")
    {
        try
        {
            var message = $"خوش آمدید {userName}!\n\nحساب کاربری شما در CMMS Identity Service با موفقیت ایجاد شد.";
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Welcome SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Failed to send welcome SMS: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SendSecurityAlertAsync(string phoneNumber, string alertMessage, string template = "security-alert")
    {
        try
        {
            var message = $"هشدار امنیتی:\n{alertMessage}\n\nCMMS Identity Service";
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Security alert SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send security alert SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"Failed to send security alert SMS: {ex.Message}");
        }
    }

    private async Task<Result<bool>> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Validate phone number format (Iranian mobile numbers)
            if (!IsValidIranianPhoneNumber(phoneNumber))
            {
                return Result<bool>.Failure("Invalid Iranian phone number format");
            }

            // Prepare the URL for Kavenegar API
            var url = $"{_baseUrl}/{_apiKey}/sms/send.json";
            
            // Prepare form data
            var formData = new List<KeyValuePair<string, string>>
            {
                new("receptor", phoneNumber),
                new("message", message)
            };

            var content = new FormUrlEncodedContent(formData);

            // Send HTTP POST request
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Parse the response
                var kavenegarResponse = JsonSerializer.Deserialize<KavenegarResponse>(responseContent);
                
                if (kavenegarResponse?.Return?.Status == 200)
                {
                    return Result<bool>.Success(true);
                }
                else
                {
                    var errorMessage = kavenegarResponse?.Return?.Message ?? "Unknown error";
                    _logger.LogWarning("Kavenegar API returned error: {Error} for phone {PhoneNumber}", errorMessage, phoneNumber);
                    return Result<bool>.Failure($"SMS sending failed: {errorMessage}");
                }
            }
            else
            {
                _logger.LogWarning("Kavenegar API HTTP error: {StatusCode} for phone {PhoneNumber}", response.StatusCode, phoneNumber);
                return Result<bool>.Failure($"SMS sending failed with HTTP status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {PhoneNumber}", phoneNumber);
            return Result<bool>.Failure($"SMS sending exception: {ex.Message}");
        }
    }

    private bool IsValidIranianPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove any non-digit characters
        var cleanNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // Iranian mobile numbers should be 11 digits starting with 09
        if (cleanNumber.Length == 11 && cleanNumber.StartsWith("09"))
            return true;
            
        // Also accept numbers starting with +98 or 98
        if (cleanNumber.Length == 12 && cleanNumber.StartsWith("989"))
            return true;
            
        if (cleanNumber.Length == 13 && cleanNumber.StartsWith("989"))
            return true;

        return false;
    }

    // Kavenegar API response models
    private class KavenegarResponse
    {
        public KavenegarReturn? Return { get; set; }
        public List<KavenegarEntry>? Entries { get; set; }
    }

    private class KavenegarReturn
    {
        public int Status { get; set; }
        public string? Message { get; set; }
    }

    private class KavenegarEntry
    {
        public long MessageId { get; set; }
        public string? Receptor { get; set; }
        public int Cost { get; set; }
        public int Status { get; set; }
        public string? StatusText { get; set; }
    }
} 