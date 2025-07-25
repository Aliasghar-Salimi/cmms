using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Auth.Commands.MfaLogin;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Tests.Common;
using IdentityService.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IdentityService.Tests.Features.Auth.Commands.MfaLogin;

public class MfaLoginCommandHandlerTests : TestBase
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ISmsVerificationService> _mockSmsService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TestDbContext _testDbContext;
    private readonly MfaLoginCommandHandler _handler;

    public MfaLoginCommandHandlerTests()
    {
        _testDbContext = new TestDbContext();
        
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);
        
        _mockSmsService = new Mock<ISmsVerificationService>();
        _mockMapper = new Mock<IMapper>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-here-with-at-least-32-characters");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("test-audience");
        
        _handler = new MfaLoginCommandHandler(
            _mockUserManager.Object,
            _testDbContext.Context,
            _mockMapper.Object,
            _mockConfiguration.Object,
            _mockSmsService.Object);
    }

    [Fact]
    public async Task Handle_WithValidMfaCode_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var userDto = new UserDto { Id = user.Id.ToString(), Email = user.Email };
        _mockMapper.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Should().NotBeNull();
        result.Data.Roles.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidMfaToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "invalid-mfa-token",
            VerificationCode = "123456"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired MFA session.");
    }

    [Fact]
    public async Task Handle_WithInvalidVerificationCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "wrong-code"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = "123456", // Different code
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid verification code.");
    }

    [Fact]
    public async Task Handle_WithExpiredCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5), // Expired
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Verification code has expired.");
    }

    [Fact]
    public async Task Handle_WithUsedCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = true, // Already used
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Verification code has already been used.");
    }

    [Fact]
    public async Task Handle_WithMaxAttemptsExceeded_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 3, // Max attempts reached
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Maximum verification attempts exceeded.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = false; // Inactive user

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User account is deactivated.");
    }

    [Fact]
    public async Task Handle_WithInvalidPurpose_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "password-reset", // Wrong purpose
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired MFA session.");
    }

    [Fact]
    public async Task Handle_WithInactiveSession_ShouldReturnFailure()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = false, // Inactive session
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid or expired MFA session.");
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldMarkCodeAsUsed()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-mfa-token",
            VerificationCode = "123456"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        var mfaSession = new SmsVerificationCode
        {
            Id = Guid.NewGuid(),
            MfaToken = command.MfaToken,
            Code = command.VerificationCode,
            Purpose = "mfa-login",
            IsActive = true,
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };

        await _testDbContext.Context.SmsVerificationCodes.AddAsync(mfaSession);
        await _testDbContext.Context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var userDto = new UserDto { Id = user.Id.ToString(), Email = user.Email };
        _mockMapper.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify code was marked as used
        var updatedSession = await _testDbContext.Context.SmsVerificationCodes
            .FirstOrDefaultAsync(s => s.MfaToken == command.MfaToken);
        updatedSession.Should().NotBeNull();
        updatedSession!.IsUsed.Should().BeTrue();
        updatedSession.UsedAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 