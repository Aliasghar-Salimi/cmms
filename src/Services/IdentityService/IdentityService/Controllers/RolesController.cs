using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Features.Roles.Commands.CreateRole;
using IdentityService.Application.Features.Roles.Commands.UpdateRole;
using IdentityService.Application.Features.Roles.Commands.DeleteRole;
using IdentityService.Application.Features.Roles.Commands.ToggleRoleStatus;
using IdentityService.Application.Features.Roles.Queries.GetRole;
using IdentityService.Application.Features.Roles.Queries.GetRoles;

namespace IdentityService.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all roles with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetRoles([FromQuery] GetRolesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetRole(string id)
    {
        var query = new GetRoleQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        var command = new CreateRoleCommand
        {
            Name = createRoleDto.Name,
            Description = createRoleDto.Description,
            TenantId = createRoleDto.TenantId,
            PermissionIds = createRoleDto.PermissionIds
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetRole), new { id = result.Value.Id }, result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        var command = new UpdateRoleCommand
        {
            Id = id,
            Name = updateRoleDto.Name,
            Description = updateRoleDto.Description,
            PermissionIds = updateRoleDto.PermissionIds
        };

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var command = new DeleteRoleCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Toggle role active status
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ToggleRoleStatus(string id)
    {
        var command = new ToggleRoleStatusCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get roles by tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(typeof(List<RoleDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetRolesByTenant(string tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetRolesQuery
        {
            TenantId = tenantId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Get active roles
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<RoleDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetActiveRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetRolesQuery
        {
            IsActive = true,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
} 