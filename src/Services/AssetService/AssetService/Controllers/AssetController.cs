using Microsoft.AspNetCore.Mvc;
using MediatR;
using AssetService.Application.Features.Asset.Commands.CreateAsset;
using AssetService.Application.Features.Asset.Commands.UpdateAsset;
using AssetService.Application.Features.Asset.Commands.DeleteAsset;
using AssetService.Application.Features.Asset.Queries.GetAssetById;
using AssetService.Application.Features.Asset.Queries.GetAssets;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Application.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace AssetService.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Produces("application/json")]
[SwaggerTag("Asset management operations")]
public class AssetController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new asset
    /// </summary>
    /// <param name="command">Asset creation data</param>
    /// <returns>Created asset details</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new asset", Description = "Creates a new asset using Saga orchestration with permission validation.")]
    [SwaggerResponse(201, "Asset created successfully", typeof(AssetDto))]
    [SwaggerResponse(400, "Invalid input data")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Forbidden - Insufficient permissions")]
    public async Task<ActionResult<ApiResponse<AssetDto>>> CreateAsset([FromBody] CreateAssetCommand command)
    {
        // Extract user token from Authorization header
        var userToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        command.UserToken = userToken;

        var result = await _mediator.Send(command);
        var response = ApiResponse<AssetDto>.SuccessResult(result, "Asset created successfully");
        return CreatedAtAction(nameof(GetAssetById), new { id = result.Id, version = "1.0" }, response);
    }

    /// <summary>
    /// Retrieves an asset by ID
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Asset details</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get asset by ID", Description = "Retrieves an asset by its unique identifier.")]
    [SwaggerResponse(200, "Asset found", typeof(AssetDto))]
    [SwaggerResponse(404, "Asset not found")]
    public async Task<ActionResult<ApiResponse<AssetDto>>> GetAssetById(Guid id)
    {
        var query = new GetAssetByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(ApiResponse<AssetDto>.NotFoundResult("Asset not found"));

        var response = ApiResponse<AssetDto>.SuccessResult(result, "Asset retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a list of assets with filtering and pagination
    /// </summary>
    /// <param name="query">Filter and pagination parameters</param>
    /// <returns>Paginated list of assets</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Get assets list", Description = "Retrieves a paginated list of assets with optional filtering.")]
    [SwaggerResponse(200, "Assets retrieved successfully", typeof(GetAssetsResponse))]
    public async Task<ActionResult<ApiResponse<GetAssetsResponse>>> GetAssets([FromQuery] GetAssetsQuery query)
    {
        var result = await _mediator.Send(query);
        var response = ApiResponse<GetAssetsResponse>.SuccessResult(result, "Assets retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Updates an existing asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="command">Asset update data</param>
    /// <returns>Updated asset details</returns>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update asset", Description = "Updates an existing asset using Saga orchestration with permission validation.")]
    [SwaggerResponse(200, "Asset updated successfully", typeof(AssetDto))]
    [SwaggerResponse(400, "Invalid input data")]
    [SwaggerResponse(404, "Asset not found")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Forbidden - Insufficient permissions")]
    public async Task<ActionResult<ApiResponse<AssetDto>>> UpdateAsset(Guid id, [FromBody] UpdateAssetCommand command)
    {
        // Extract user token from Authorization header
        var userToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        command.Id = id;
        command.UserToken = userToken;

        var result = await _mediator.Send(command);
        var response = ApiResponse<AssetDto>.SuccessResult(result, "Asset updated successfully");
        return Ok(response);
    }

    /// <summary>
    /// Deletes an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete asset", Description = "Deletes an asset using Saga orchestration with permission validation.")]
    [SwaggerResponse(200, "Asset deleted successfully")]
    [SwaggerResponse(404, "Asset not found")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Forbidden - Insufficient permissions")]
    public async Task<ActionResult<ApiResponse>> DeleteAsset(Guid id)
    {
        // Extract user token from Authorization header
        var userToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        var command = new DeleteAssetCommand { Id = id, UserToken = userToken };
        var result = await _mediator.Send(command);
        
        if (result)
            return Ok(ApiResponse.SuccessResult("Asset deleted successfully"));
        
        return BadRequest(ApiResponse.ErrorResult("Failed to delete asset"));
    }

    /// <summary>
    /// Health check endpoint for assets
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [SwaggerOperation(Summary = "Asset service health check", Description = "Public health check endpoint for the asset service")]
    [SwaggerResponse(200, "Service is healthy")]
    public ActionResult Health()
    {
        return Ok(new { 
            service = "Asset Service", 
            status = "Healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0"
        });
    }
}

