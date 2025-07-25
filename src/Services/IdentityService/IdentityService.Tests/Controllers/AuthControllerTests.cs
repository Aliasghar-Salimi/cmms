using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.MfaLogin;
using IdentityService.Application.Features.Auth.Commands.RefreshToken;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.ChangePassword;
using IdentityService.Application.Features.Auth.Commands.ForgotPassword;
using IdentityService.Application.Features.Auth.Commands.ResetPassword;
using IdentityService.Application.Features.Auth.Commands.EnableMfa;
using IdentityService.Application.Features.Auth.Commands.VerifyMfa;
using IdentityService.Application.Features.Auth.Commands.ResendOtp;
using IdentityService.Application.Features.Auth.Queries.GetCurrentUser;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;
using IdentityService.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace IdentityService.Tests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new AuthController(_mockMediator.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var response = new MfaLoginResponseDto
        {
            RequiresMfa = false,
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<MfaLoginResponseDto>.Success(response));

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<MfaLoginResponseDto>.Failure("Invalid credentials"));

        // Act
        var result = await _controller.Login(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid credentials" });
    }

    [Fact]
    public async Task MfaLogin_WithValidCode_ShouldReturnOkResult()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-token",
            VerificationCode = "123456"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<LoginResponseDto>.Success(response));

        // Act
        var result = await _controller.MfaLogin(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task MfaLogin_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new MfaLoginCommand
        {
            MfaToken = "valid-token",
            VerificationCode = "wrong-code"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<LoginResponseDto>.Failure("Invalid verification code"));

        // Act
        var result = await _controller.MfaLogin(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid verification code" });
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnOkResult()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        var response = new LoginResponseDto
        {
            AccessToken = "new-jwt-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<LoginResponseDto>.Success(response));

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid-refresh-token"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<LoginResponseDto>.Failure("Invalid refresh token"));

        // Act
        var result = await _controller.RefreshToken(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid refresh token" });
    }

    [Fact]
    public async Task Logout_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new LogoutCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.Logout(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task Logout_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new LogoutCommand
        {
            RefreshToken = "invalid-refresh-token"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Invalid refresh token"));

        // Act
        var result = await _controller.Logout(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid refresh token" });
    }

    [Fact]
    public async Task ChangePassword_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ChangePassword(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword123!"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Current password is incorrect"));

        // Act
        var result = await _controller.ChangePassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Current password is incorrect" });
    }

    [Fact]
    public async Task ForgotPassword_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
                    var command = new ForgotPasswordCommand
            {
                PhoneNumber = "+1234567890"
            };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            PhoneNumber = "+1234567890"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("User not found"));

        // Act
        var result = await _controller.ForgotPassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "User not found" });
    }

    [Fact]
    public async Task ResetPassword_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            PhoneNumber = "+1234567890",
            VerificationCode = "123456",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            PhoneNumber = "+1234567890",
            VerificationCode = "wrong-code",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Invalid verification code"));

        // Act
        var result = await _controller.ResetPassword(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid verification code" });
    }

    [Fact]
    public async Task EnableMfa_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new EnableMfaCommand
        {
            PhoneNumber = "+1234567890"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.EnableMfa(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task EnableMfa_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new EnableMfaCommand
        {
            PhoneNumber = "invalid-phone"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Invalid phone number"));

        // Act
        var result = await _controller.EnableMfa(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid phone number" });
    }

    [Fact]
    public async Task VerifyMfa_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new VerifyMfaCommand
        {
            VerificationCode = "123456"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.VerifyMfa(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task VerifyMfa_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new VerifyMfaCommand
        {
            VerificationCode = "wrong-code"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Invalid verification code"));

        // Act
        var result = await _controller.VerifyMfa(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid verification code" });
    }

    [Fact]
    public async Task ResendOtp_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var command = new ResendOtpCommand
        {
            PhoneNumber = "+1234567890",
            Purpose = "mfa-login"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _controller.ResendOtp(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task ResendOtp_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ResendOtpCommand
        {
            PhoneNumber = "invalid-phone",
            Purpose = "mfa-login"
        };

        _mockMediator.Setup(x => x.Send(command, CancellationToken.None))
            .ReturnsAsync(Result<bool>.Failure("Invalid phone number"));

        // Act
        var result = await _controller.ResendOtp(command);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeEquivalentTo(new { error = "Invalid phone number" });
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnOkResult()
    {
        // Arrange
        var response = new CurrentUserDto
        {
            User = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "testuser",
                Email = "test@example.com"
            },
            Roles = new List<string> { "User" },
            Permissions = new List<string> { "User:Read" },
            HasMfaEnabled = false,
            TokenExpiresAt = DateTime.UtcNow.AddHours(1),
            TenantId = Guid.NewGuid().ToString(),
            TenantName = "TestTenant"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<GetCurrentUserQuery>(), CancellationToken.None))
            .ReturnsAsync(Result<CurrentUserDto>.Success(response));

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockMediator.Setup(x => x.Send(It.IsAny<GetCurrentUserQuery>(), CancellationToken.None))
            .ReturnsAsync(Result<CurrentUserDto>.Failure("Invalid token"));

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().BeEquivalentTo(new 
        { 
            error = "Invalid token",
            message = "Authentication failed",
            statusCode = 401
        });
    }
} 