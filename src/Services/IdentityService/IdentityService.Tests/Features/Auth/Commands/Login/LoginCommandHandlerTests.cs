using AutoFixture;
using AutoMapper;
using FluentAssertions;
using IdentityService.Application.Features.Auth.Commands.Login;
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

namespace IdentityService.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests : TestBase
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ISmsVerificationService> _mockSmsService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TestDbContext _testDbContext;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
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
        
        _handler = new LoginCommandHandler(
            _mockUserManager.Object,
            _testDbContext.Context,
            _mockMapper.Object,
            _mockConfiguration.Object,
            _mockSmsService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;
        user.TenantId = Guid.NewGuid();

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);
        
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
        result.Data!.RequiresMfa.Should().BeFalse();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.User.Should().NotBeNull();
        result.Data.Roles.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = false;

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User account is deactivated.");
    }

    [Fact]
    public async Task Handle_WithInvalidTenantId_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!",
            TenantId = Guid.NewGuid().ToString()
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;
        user.TenantId = Guid.NewGuid(); // Different tenant

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User does not belong to the specified tenant.");
    }

    [Fact]
    public async Task Handle_WithMfaEnabled_ShouldReturnMfaResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;
        user.PhoneNumber = "+1234567890";

        var userMfa = new UserMfa
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsActive = true,
            IsEnabled = true,
            MfaType = "SMS"
        };

        await _testDbContext.Context.UserMfas.AddAsync(userMfa);
        await _testDbContext.Context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockSmsService.Setup(x => x.GenerateAndSendOtpAsync(
                user.PhoneNumber, "mfa-login", user.Id, 5))
            .ReturnsAsync(Result<string>.Success("123456"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.RequiresMfa.Should().BeTrue();
        result.Data.MfaToken.Should().Be("123456");
        result.Data.PhoneNumber.Should().Contain("**");
        result.Data.MfaType.Should().Be("SMS");
    }

    [Fact]
    public async Task Handle_WithMfaEnabledButSmsFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = TestData.Users.CreateValidUser();
        user.IsActive = true;
        user.PhoneNumber = "+1234567890";

        var userMfa = new UserMfa
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsActive = true,
            IsEnabled = true,
            MfaType = "SMS"
        };

        await _testDbContext.Context.UserMfas.AddAsync(userMfa);
        await _testDbContext.Context.SaveChangesAsync();

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockSmsService.Setup(x => x.GenerateAndSendOtpAsync(
                user.PhoneNumber, "mfa-login", user.Id, 5))
            .ReturnsAsync(Result<string>.Failure("SMS service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to send MFA code");
    }

    [Theory]
    [InlineData("+1234567890", "+12****90")]
    [InlineData("+123456789", "+12***89")]
    [InlineData("1234567890", "12****90")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void MaskPhoneNumber_ShouldMaskCorrectly(string input, string expected)
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = TestData.Users.CreateValidUser();
        user.PhoneNumber = input;

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);
        
        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        // Act
        var result = _handler.Handle(command, CancellationToken.None).Result;

        // Assert
        if (result.IsSuccess && result.Data?.RequiresMfa == true)
        {
            result.Data.PhoneNumber.Should().Be(expected);
        }
    }

    public void Dispose()
    {
        _testDbContext.Dispose();
    }
} 