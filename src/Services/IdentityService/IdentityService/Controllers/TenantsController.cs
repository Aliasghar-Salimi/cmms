using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Features.Tenants.Commands.CreateTenant;
using IdentityService.Application.Features.Tenants.Commands.UpdateTenant;
using IdentityService.Application.Features.Tenants.Commands.DeleteTenant;
using IdentityService.Application.Features.Tenants.Queries.GetTenantById;
using IdentityService.Application.Features.Tenants.Queries.GetTenants;
using IdentityService.Application.Features.Tenants.DTOs;

namespace IdentityService.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    // GUIDANCE: dependency injection, mediator parameter is in the constructor and _mediator is the field
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all tenants with filtering and pagination
    /// </summary>
    // GUIDANCE: HttpGet is the method, [FromQuery] is the query parameter, ProducesResponseType is the response type, 200 is the status code, 400 is the bad request, 401 is the unauthorized
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantDto>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetTenants([FromQuery] GetTenantsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TenantDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        var query = new GetTenantByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
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

        return CreatedAtAction(nameof(GetTenantById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TenantDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto updateDto)
    {
        var command = new UpdateTenantCommand
        {
            Id = id,
            Name = updateDto.Name,
            Description = updateDto.Description,
            IsActive = updateDto.IsActive
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
    /// Delete a tenant
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var command = new DeleteTenantCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Activate/Deactivate a tenant
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    [ProducesResponseType(typeof(TenantDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ToggleTenantStatus(Guid id)
    {
        // First get the current tenant
        var getTenantQuery = new GetTenantByIdQuery { Id = id };
        var tenantResult = await _mediator.Send(getTenantQuery);
        
        if (!tenantResult.IsSuccess)
        {
            return NotFound(tenantResult);
        }

        var tenant = tenantResult.Data!;
        var updateCommand = new UpdateTenantCommand
        {
            Id = id,
            Name = tenant.Name,
            Description = tenant.Description,
            IsActive = !tenant.IsActive // Toggle the status
        };

        var result = await _mediator.Send(updateCommand);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get tenant statistics
    /// </summary>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetTenantStatistics(Guid id)
    {
        var query = new GetTenantByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        var tenant = result.Data!;
        var statistics = new
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            UserCount = tenant.UserCount,
            RoleCount = tenant.RoleCount,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };

        return Ok(statistics);
    }
} 
