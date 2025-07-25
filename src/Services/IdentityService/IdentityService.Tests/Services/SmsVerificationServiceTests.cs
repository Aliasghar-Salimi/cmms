using FluentAssertions;
using IdentityService.Application.Common.Services;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdentityService.Tests.Services;

public class SmsVerificationServiceTests : TestBase
{
    private readonly Mock<ISmsService> _mockSmsService;
    private readonly Mock<ILogger<SmsVerificationService>> _mockLogger;
    private readonly TestDbContext _testDbContext;
    private readonly SmsVerificationService _service;

    public SmsVerificationServiceTests()
    {
        _testDbContext = new TestDbContext();
        _mockSmsService = new Mock<ISmsService>();
        _mockLogger = new Mock<ILogger<SmsVerificationService>>();
        
        _service = new SmsVerificationService(
            _testDbContext.Context,
            _mockSmsService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateAndSendOtpAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var userId = Guid.NewGuid();

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _service.GenerateAndSendOtpAsync(phoneNumber, purpose, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().HaveLength(6); // 6-digit OTP
    }

    [Fact]
    public async Task GenerateAndSendOtpAsync_WithMfaPurpose_ShouldReturnMfaToken()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "mfa-login";
        var userId = Guid.NewGuid();

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _service.GenerateAndSendOtpAsync(phoneNumber, purpose, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().HaveLength(36); // GUID length
        Guid.TryParse(result.Data, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAndSendOtpAsync_WithSmsFailure_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Failure("SMS service unavailable"));

        // Act
        var result = await _service.GenerateAndSendOtpAsync(phoneNumber, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to send OTP");
    }

    [Fact]
    public async Task GenerateAndSendOtpAsync_ShouldInvalidateExistingCodes()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";

        var existingCode = TestData.SmsVerificationCodes.CreateValidCode();
        existingCode.PhoneNumber = phoneNumber;
        existingCode.Purpose = purpose;
        existingCode.IsActive = true;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(existingCode);
        await _testDbContext.Context.SaveChangesAsync();

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _service.GenerateAndSendOtpAsync(phoneNumber, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify existing code was invalidated
        var invalidatedCode = await _testDbContext.Context.SmsVerificationCodes
            .FirstOrDefaultAsync(c => c.Id == existingCode.Id);
        invalidatedCode.Should().NotBeNull();
        invalidatedCode!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_WithValidCode_ShouldReturnSuccess()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var code = "123456";

        var verificationCode = TestData.SmsVerificationCodes.CreateValidCode();
        verificationCode.PhoneNumber = phoneNumber;
        verificationCode.Purpose = purpose;
        verificationCode.Code = code;
        verificationCode.IsActive = true;
        verificationCode.IsUsed = false;
        verificationCode.Attempts = 0;
        verificationCode.MaxAttempts = 3;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(verificationCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        // Verify code was marked as used
        var usedCode = await _testDbContext.Context.SmsVerificationCodes
            .FirstOrDefaultAsync(c => c.Id == verificationCode.Id);
        usedCode.Should().NotBeNull();
        usedCode!.IsUsed.Should().BeTrue();
        usedCode.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtpAsync_WithInvalidCode_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var correctCode = "123456";
        var wrongCode = "654321";

        var verificationCode = TestData.SmsVerificationCodes.CreateValidCode();
        verificationCode.PhoneNumber = phoneNumber;
        verificationCode.Purpose = purpose;
        verificationCode.Code = correctCode;
        verificationCode.IsActive = true;
        verificationCode.IsUsed = false;
        verificationCode.Attempts = 0;
        verificationCode.MaxAttempts = 3;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(verificationCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, wrongCode, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid verification code");

        // Verify attempts were incremented
        var updatedCode = await _testDbContext.Context.SmsVerificationCodes
            .FirstOrDefaultAsync(c => c.Id == verificationCode.Id);
        updatedCode.Should().NotBeNull();
        updatedCode!.Attempts.Should().Be(1);
    }

    [Fact]
    public async Task VerifyOtpAsync_WithExpiredCode_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var code = "123456";

        var verificationCode = TestData.SmsVerificationCodes.CreateValidCode();
        verificationCode.PhoneNumber = phoneNumber;
        verificationCode.Purpose = purpose;
        verificationCode.Code = code;
        verificationCode.IsActive = true;
        verificationCode.IsUsed = false;
        verificationCode.Attempts = 0;
        verificationCode.MaxAttempts = 3;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(verificationCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Verification code has expired");
    }

    [Fact]
    public async Task VerifyOtpAsync_WithUsedCode_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var code = "123456";

        var verificationCode = TestData.SmsVerificationCodes.CreateValidCode();
        verificationCode.PhoneNumber = phoneNumber;
        verificationCode.Purpose = purpose;
        verificationCode.Code = code;
        verificationCode.IsActive = true;
        verificationCode.IsUsed = true; // Already used
        verificationCode.Attempts = 0;
        verificationCode.MaxAttempts = 3;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(verificationCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Verification code has already been used");
    }

    [Fact]
    public async Task VerifyOtpAsync_WithMaxAttemptsExceeded_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var code = "123456";

        var verificationCode = TestData.SmsVerificationCodes.CreateValidCode();
        verificationCode.PhoneNumber = phoneNumber;
        verificationCode.Purpose = purpose;
        verificationCode.Code = code;
        verificationCode.IsActive = true;
        verificationCode.IsUsed = false;
        verificationCode.Attempts = 3; // Max attempts reached
        verificationCode.MaxAttempts = 3;

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(verificationCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Too many verification attempts. Please request a new code.");
    }

    [Fact]
    public async Task VerifyOtpAsync_WithNonExistentCode_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var code = "123456";

        // Act
        var result = await _service.VerifyOtpAsync(phoneNumber, code, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired verification code");
    }

    [Fact]
    public async Task ResendOtpAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var userId = Guid.NewGuid();

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _service.ResendOtpAsync(phoneNumber, purpose, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ResendOtpAsync_WithRecentCode_ShouldReturnFailure()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";

        var recentCode = TestData.SmsVerificationCodes.CreateValidCode();
        recentCode.PhoneNumber = phoneNumber;
        recentCode.Purpose = purpose;
        recentCode.IsActive = true;
        recentCode.CreatedAt = DateTime.UtcNow.AddMinutes(-30); // Recent code

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(recentCode);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.ResendOtpAsync(phoneNumber, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Please wait at least 1 minute before requesting a new code");
    }

    [Fact]
    public async Task InvalidateOtpAsync_WithActiveCodes_ShouldInvalidateAll()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";

        var codes = new List<SmsVerificationCode>
        {
            TestData.SmsVerificationCodes.CreateValidCode(),
            TestData.SmsVerificationCodes.CreateValidCode(),
            TestData.SmsVerificationCodes.CreateValidCode()
        };

        foreach (var code in codes)
        {
            code.PhoneNumber = phoneNumber;
            code.Purpose = purpose;
            code.IsActive = true;
        }

        await _testDbContext.Context.SmsVerificationCodes.AddRangeAsync(codes);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.InvalidateOtpAsync(phoneNumber, purpose);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        // Verify all codes were invalidated
        var invalidatedCodes = await _testDbContext.Context.SmsVerificationCodes
            .Where(c => c.PhoneNumber == phoneNumber && c.Purpose == purpose)
            .ToListAsync();

        invalidatedCodes.Should().HaveCount(3);
        invalidatedCodes.Should().OnlyContain(c => !c.IsActive);
    }

    [Fact]
    public async Task CleanupExpiredCodesAsync_WithExpiredCodes_ShouldCleanupAll()
    {
        // Arrange
        var expiredCodes = new List<SmsVerificationCode>
        {
            TestData.SmsVerificationCodes.CreateValidCode(),
            TestData.SmsVerificationCodes.CreateValidCode(),
            TestData.SmsVerificationCodes.CreateValidCode()
        };

        foreach (var code in expiredCodes)
        {
            code.IsActive = true;
            code.ExpiresAt = DateTime.UtcNow.AddMinutes(-10); // Expired
        }

        await _testDbContext.Context.SmsVerificationCodes.AddRangeAsync(expiredCodes);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _service.CleanupExpiredCodesAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        // Verify all expired codes were cleaned up
        var cleanedCodes = await _testDbContext.Context.SmsVerificationCodes
            .Where(c => expiredCodes.Select(ec => ec.Id).Contains(c.Id))
            .ToListAsync();

        cleanedCodes.Should().HaveCount(3);
        cleanedCodes.Should().OnlyContain(c => !c.IsActive);
    }

    [Fact]
    public async Task GenerateAndSendOtpAsync_ShouldSetCorrectExpiryTime()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var purpose = "password-reset";
        var expiryMinutes = 10;

        _mockSmsService.Setup(x => x.SendOtpAsync(phoneNumber, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _service.GenerateAndSendOtpAsync(phoneNumber, purpose, null, expiryMinutes);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify expiry time was set correctly
        var verificationCode = await _testDbContext.Context.SmsVerificationCodes
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.Purpose == purpose);

        verificationCode.Should().NotBeNull();
        verificationCode!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(expiryMinutes), TimeSpan.FromMinutes(1));
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 