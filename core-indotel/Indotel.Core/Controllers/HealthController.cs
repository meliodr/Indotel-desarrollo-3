using Microsoft.AspNetCore.Mvc;

namespace Indotel.Core.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "OK",
            service = "INDOTEL Core API",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
