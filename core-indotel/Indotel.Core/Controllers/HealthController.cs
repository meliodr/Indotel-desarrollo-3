using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
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
    [HttpGet("live")]
    [HttpGet("~/health/live")]
    public IActionResult Live()
    {
        return Ok(new
        {
            status = "OK",
            service = "INDOTEL Core API",
            version = "1.1.0",
            environment = _environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            correlationId = HttpContext.TraceIdentifier
        });
    }

    [AllowAnonymous]
    [HttpGet("ready")]
    [HttpGet("db")]
    [HttpGet("~/health/ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        try
        {
            var puedeConectar = await _db.Database.CanConnectAsync(cancellationToken);

            var payload = new
            {
                status = puedeConectar ? "OK" : "ERROR",
                service = "INDOTEL Core API",
                database = puedeConectar ? "CONNECTED" : "DISCONNECTED",
                timestamp = DateTime.UtcNow,
                correlationId = HttpContext.TraceIdentifier
            };

            return puedeConectar
                ? Ok(payload)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "ERROR",
                service = "INDOTEL Core API",
                database = "DISCONNECTED",
                codigo = "BASE_DATOS_NO_DISPONIBLE",
                mensaje = "El Core está activo, pero la base de datos no está disponible",
                timestamp = DateTime.UtcNow,
                correlationId = HttpContext.TraceIdentifier
            });
        }
    }
}
