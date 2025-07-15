using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            Message = "CMMS Identity Service is running",
            Endpoints = new {
                Health = "/health",
                Swagger = "/swagger",
                Api = "/api"
            },
            Timestamp = DateTime.UtcNow
        });
    }
} 