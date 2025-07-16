using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Features.Users.Commands.CreateUser;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Features.Users.Commands.DeleteUser;
using IdentityService.Application.Features.Users.Quesries.GetUserById;
using IdentityService.Application.Features.Users.Quesries.GetUsers;
using IdentityService.Application.DTOs;

namespace IdentityService.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Any())
            {
                return BadRequest(result);
            }
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        var command = new UpdateUserCommand
        {
            Id = id,
            UserName = updateDto.UserName,
            Email = updateDto.Email,
            PhoneNumber = updateDto.PhoneNumber,
            EmailConfirmed = updateDto.EmailConfirmed,
            PhoneNumberConfirmed = updateDto.PhoneNumberConfirmed,
            TwoFactorEnabled = updateDto.TwoFactorEnabled,
            LockoutEnabled = updateDto.LockoutEnabled,
            IsActive = updateDto.IsActive,
            RoleIds = updateDto.RoleIds
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Any())
            {
                return BadRequest(result);
            }
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Activate/Deactivate a user
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        // First get the current user
        var getUserQuery = new GetUserByIdQuery { Id = id };
        var userResult = await _mediator.Send(getUserQuery);
        
        if (!userResult.IsSuccess)
        {
            return NotFound(userResult);
        }

        var user = userResult.Data!;
        var updateCommand = new UpdateUserCommand
        {
            Id = id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,
            IsActive = !user.IsActive, // Toggle the status
            RoleIds = user.Roles.Select(r => Guid.Parse(r)).ToList() // Convert role names back to IDs (simplified)
        };

        var result = await _mediator.Send(updateCommand);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }
} 