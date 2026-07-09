using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IndotelDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public HealthController(IndotelDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpGet]
    [HttpGet("~/health")]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "OK",
            service = "INDOTEL Core API",
            version = "1.0.0",
            environment = _environment.EnvironmentName,
            timestamp = DateTime.UtcNow
        });
    }

    [AllowAnonymous]
    [HttpGet("db")]
    public async Task<IActionResult> Db()
    {
        var puedeConectar = await _db.Database.CanConnectAsync();

        return Ok(new
        {
            status = puedeConectar ? "OK" : "ERROR",
            database = puedeConectar ? "CONNECTED" : "DISCONNECTED",
            timestamp = DateTime.UtcNow
        });
    }
}
