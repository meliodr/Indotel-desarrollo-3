using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ReportesController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var data = new
        {
            ciudadanos = await _db.Ciudadanos.CountAsync(),
            prestadoras = await _db.Prestadoras.CountAsync(),
            servicios = await _db.ServiciosTelecom.CountAsync(),
            reclamaciones = await _db.Reclamaciones.CountAsync(),
            abiertas = await _db.Reclamaciones.CountAsync(x => x.Estado != "CERRADA" && x.Estado != "RESUELTA"),
            cerradas = await _db.Reclamaciones.CountAsync(x => x.Estado == "CERRADA" || x.Estado == "RESUELTA")
        };

        return Ok(data);
    }

    [HttpGet("reclamaciones-por-estado")]
    public async Task<IActionResult> ReclamacionesPorEstado()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("reclamaciones-por-prestadora")]
    public async Task<IActionResult> ReclamacionesPorPrestadora()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.PrestadoraId)
            .Select(x => new { prestadoraId = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }
}
