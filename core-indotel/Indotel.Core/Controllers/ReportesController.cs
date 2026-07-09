using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
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
            cerradas = await _db.Reclamaciones.CountAsync(x => x.Estado == "CERRADA" || x.Estado == "RESUELTA"),
            vencidas = await _db.Reclamaciones.CountAsync(x => x.EstaVencida),
            respondidasPrestadora = await _db.Reclamaciones.CountAsync(x => x.FechaRespuestaPrestadora != null),
            resueltas = await _db.Reclamaciones.CountAsync(x => x.Estado == "RESUELTA"),
            cerradasFinales = await _db.Reclamaciones.CountAsync(x => x.Estado == "CERRADA"),
            resolucionesInstitucionales = await _db.ResolucionesInstitucionales.CountAsync()
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

    [HttpGet("reclamaciones-por-servicio")]
    public async Task<IActionResult> ReclamacionesPorServicio()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.ServicioTelecomId)
            .Select(x => new { servicioTelecomId = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("reclamaciones-por-provincia")]
    public async Task<IActionResult> ReclamacionesPorProvincia()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.Provincia)
            .Select(x => new { provincia = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("reclamaciones-por-tipo")]
    public async Task<IActionResult> ReclamacionesPorTipo()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.TipoReclamacionId)
            .Select(x => new { tipoReclamacionId = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("sla")]
    public async Task<IActionResult> Sla()
    {
        var totalEnviadas = await _db.Reclamaciones.CountAsync(x => x.FechaEnvioPrestadora != null);
        var respondidas = await _db.Reclamaciones.CountAsync(x => x.FechaRespuestaPrestadora != null);
        var vencidas = await _db.Reclamaciones.CountAsync(x => x.EstaVencida);
        var pendientesRespuesta = await _db.Reclamaciones.CountAsync(x =>
            x.FechaEnvioPrestadora != null &&
            x.FechaRespuestaPrestadora == null &&
            !x.EstaVencida);

        var data = new
        {
            totalEnviadas,
            respondidas,
            pendientesRespuesta,
            vencidas,
            porcentajeRespondidas = totalEnviadas == 0 ? 0 : Math.Round((decimal)respondidas / totalEnviadas * 100, 2),
            porcentajeVencidas = totalEnviadas == 0 ? 0 : Math.Round((decimal)vencidas / totalEnviadas * 100, 2)
        };

        return Ok(data);
    }

    [HttpGet("productividad")]
    public async Task<IActionResult> Productividad([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var query = _db.Reclamaciones.AsQueryable();

        if (desde is not null)
        {
            query = query.Where(x => x.FechaCreacion >= desde.Value);
        }

        if (hasta is not null)
        {
            query = query.Where(x => x.FechaCreacion <= hasta.Value);
        }

        var data = new
        {
            recibidas = await query.CountAsync(),
            enviadasPrestadora = await query.CountAsync(x => x.FechaEnvioPrestadora != null),
            respondidasPrestadora = await query.CountAsync(x => x.FechaRespuestaPrestadora != null),
            resueltas = await query.CountAsync(x => x.FechaResolucion != null),
            cerradas = await query.CountAsync(x => x.FechaCierre != null)
        };

        return Ok(data);
    }

    [HttpGet("resoluciones")]
    public async Task<IActionResult> Resoluciones()
    {
        var porEstado = await _db.ResolucionesInstitucionales
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var porTipo = await _db.ResolucionesInstitucionales
            .GroupBy(x => x.TipoResolucionId)
            .Select(x => new { tipoResolucionId = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var data = new
        {
            total = await _db.ResolucionesInstitucionales.CountAsync(),
            borrador = await _db.ResolucionesInstitucionales.CountAsync(x => x.Estado == "BORRADOR"),
            aprobadas = await _db.ResolucionesInstitucionales.CountAsync(x => x.Estado == "APROBADA"),
            publicadas = await _db.ResolucionesInstitucionales.CountAsync(x => x.Estado == "PUBLICADA"),
            archivadas = await _db.ResolucionesInstitucionales.CountAsync(x => x.Estado == "ARCHIVADA"),
            porEstado,
            porTipo
        };

        return Ok(data);
    }
}
