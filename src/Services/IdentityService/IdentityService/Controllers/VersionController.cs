using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Common;

namespace IdentityService.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VersionController : ControllerBase
{
    /// <summary>
    /// Get detailed version information
    /// </summary>
    [HttpGet]
    public IActionResult GetVersion()
    {
        return Ok(VersionInfo.GetVersionObject());
    }

    /// <summary>
    /// Get simple version string
    /// </summary>
    [HttpGet("simple")]
    public IActionResult GetSimpleVersion()
    {
        return Ok(new { Version = VersionInfo.InformationalVersion });
    }

    /// <summary>
    /// Get build information
    /// </summary>
    [HttpGet("build")]
    public IActionResult GetBuildInfo()
    {
        return Ok(new
        {
            BuildVersion = VersionInfo.Version,
            FileVersion = VersionInfo.FileVersion,
            AssemblyName = VersionInfo.AssemblyName,
            BuildDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
        });
    }

    /// <summary>
    /// Get health check with version
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "CMMS Identity Service",
            Version = VersionInfo.InformationalVersion,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }
} 