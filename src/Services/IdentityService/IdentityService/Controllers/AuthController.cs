using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Features.Auth.Commands.Login;
using IdentityService.Application.Features.Auth.Commands.Logout;
using IdentityService.Application.Features.Auth.Commands.RefreshToken;
using IdentityService.Application.Features.Auth.Commands.ChangePassword;
using IdentityService.Application.Features.Auth.Commands.ForgotPassword;
using IdentityService.Application.Features.Auth.Commands.ResetPassword;
using IdentityService.Application.Features.Auth.Commands.EnableMfa;
using IdentityService.Application.Features.Auth.Commands.VerifyMfa;
using IdentityService.Application.Features.Auth.Commands.ResendOtp;
using IdentityService.Application.Features.Auth.Commands.MfaLogin;
using IdentityService.Application.Features.Auth.Queries.GetCurrentUser;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;
using IdentityService.Application.Common.Services;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[Microsoft.AspNetCore.Mvc.ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticate user and return JWT token or MFA challenge
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>Authentication response with tokens or MFA challenge</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(MfaLoginResponseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Complete MFA login with verification code
    /// </summary>
    /// <param name="command">MFA verification request</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("mfa-login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> MfaLogin([FromBody] MfaLoginCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    /// <param name="command">Refresh token request</param>
    /// <returns>New authentication response with tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Logout user and revoke refresh tokens
    /// </summary>
    /// <param name="command">Logout request</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="command">Password change request</param>
    /// <returns>Success status</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Request password reset via SMS
    /// </summary>
    /// <param name="command">Password reset request</param>
    /// <returns>Success status</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Reset password using SMS verification code
    /// </summary>
    /// <param name="command">Password reset with verification code</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Enable MFA for user
    /// </summary>
    /// <param name="command">MFA setup request</param>
    /// <returns>Success status</returns>
    [HttpPost("enable-mfa")]
    [Authorize]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> EnableMfa([FromBody] EnableMfaCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Verify MFA code
    /// </summary>
    /// <param name="command">MFA verification request</param>
    /// <returns>Success status</returns>
    [HttpPost("verify-mfa")]
    [Authorize]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Resend OTP for MFA or password reset
    /// </summary>
    /// <param name="command">OTP resend request</param>
    /// <returns>Success status</returns>
    [HttpPost("resend-otp")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get current user information from JWT token
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetCurrentUserQuery());
        
        if (!result.IsSuccess)
        {
            return Unauthorized(new { 
                error = result.Error,
                message = "Authentication failed",
                statusCode = 401
            });
        }
        
        return Ok(result.Data);
    }


}

 