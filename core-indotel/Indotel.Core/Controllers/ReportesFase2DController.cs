using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
[ApiController]
[Route("api/reportes")]
public class ReportesFase2DController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ReportesFase2DController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("prestadoras-ranking")]
    public async Task<IActionResult> PrestadorasRanking()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.PrestadoraId)
            .Select(x => new
            {
                prestadoraId = x.Key,
                prestadora = _db.Prestadoras.Where(p => p.Id == x.Key).Select(p => p.NombreComercial).FirstOrDefault(),
                totalReclamaciones = x.Count(),
                vencidas = x.Count(r => r.EstaVencida),
                resueltas = x.Count(r => r.Estado == "RESUELTA" || r.Estado == "CERRADA")
            })
            .OrderByDescending(x => x.totalReclamaciones)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("sla-ranking")]
    public async Task<IActionResult> SlaRanking()
    {
        var data = await _db.Reclamaciones
            .Where(x => x.FechaEnvioPrestadora != null)
            .GroupBy(x => x.PrestadoraId)
            .Select(x => new
            {
                prestadoraId = x.Key,
                prestadora = _db.Prestadoras.Where(p => p.Id == x.Key).Select(p => p.NombreComercial).FirstOrDefault(),
                totalEnviadas = x.Count(),
                vencidas = x.Count(r => r.EstaVencida),
                respondidas = x.Count(r => r.FechaRespuestaPrestadora != null)
            })
            .OrderByDescending(x => x.vencidas)
            .ToListAsync();

        var result = data.Select(x => new
        {
            x.prestadoraId,
            x.prestadora,
            x.totalEnviadas,
            x.vencidas,
            x.respondidas,
            porcentajeVencidas = x.totalEnviadas == 0 ? 0 : Math.Round((decimal)x.vencidas / x.totalEnviadas * 100, 2),
            porcentajeRespondidas = x.totalEnviadas == 0 ? 0 : Math.Round((decimal)x.respondidas / x.totalEnviadas * 100, 2)
        });

        return Ok(result);
    }

    [HttpGet("reclamaciones-mensual")]
    public async Task<IActionResult> ReclamacionesMensual()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => new { x.FechaCreacion.Year, x.FechaCreacion.Month })
            .Select(x => new
            {
                anio = x.Key.Year,
                mes = x.Key.Month,
                total = x.Count(),
                vencidas = x.Count(r => r.EstaVencida),
                resueltas = x.Count(r => r.Estado == "RESUELTA" || r.Estado == "CERRADA")
            })
            .OrderBy(x => x.anio)
            .ThenBy(x => x.mes)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("tiempo-promedio-respuesta")]
    public async Task<IActionResult> TiempoPromedioRespuesta()
    {
        var query = _db.Reclamaciones
            .Where(x => x.FechaEnvioPrestadora != null && x.FechaRespuestaPrestadora != null);

        var totalRespondidas = await query.CountAsync();
        var promedioDias = totalRespondidas == 0
            ? 0
            : await query.AverageAsync(x => EF.Functions.DateDiffDay(x.FechaEnvioPrestadora!.Value, x.FechaRespuestaPrestadora!.Value));

        var data = new
        {
            totalRespondidas,
            promedioDiasRespuesta = Math.Round((decimal)promedioDias, 2)
        };

        return Ok(data);
    }

    [HttpGet("servicios-mas-reclamados")]
    public async Task<IActionResult> ServiciosMasReclamados()
    {
        var data = await _db.Reclamaciones
            .GroupBy(x => x.ServicioTelecomId)
            .Select(x => new
            {
                servicioTelecomId = x.Key,
                servicio = _db.ServiciosTelecom.Where(s => s.Id == x.Key).Select(s => s.Nombre).FirstOrDefault(),
                total = x.Count(),
                vencidas = x.Count(r => r.EstaVencida)
            })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("resoluciones-periodo")]
    public async Task<IActionResult> ResolucionesPeriodo()
    {
        var data = await _db.ResolucionesInstitucionales
            .GroupBy(x => new { x.FechaCreacion.Year, x.FechaCreacion.Month })
            .Select(x => new
            {
                anio = x.Key.Year,
                mes = x.Key.Month,
                total = x.Count(),
                publicadas = x.Count(r => r.Estado == "PUBLICADA"),
                archivadas = x.Count(r => r.Estado == "ARCHIVADA")
            })
            .OrderBy(x => x.anio)
            .ThenBy(x => x.mes)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("autorizaciones-estado")]
    public async Task<IActionResult> AutorizacionesEstado()
    {
        var data = await _db.SolicitudesAutorizacion
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("certificaciones-estado")]
    public async Task<IActionResult> CertificacionesEstado()
    {
        var data = await _db.SolicitudesCertificacion
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("licencias-vencimiento")]
    public async Task<IActionResult> LicenciasVencimiento()
    {
        var hoy = DateTime.UtcNow;
        var limite = hoy.AddDays(60);

        var data = new
        {
            total = await _db.LicenciasTecnicas.CountAsync(),
            vencidas = await _db.LicenciasTecnicas.CountAsync(x => x.FechaVencimiento < hoy && x.Estado != "CANCELADA"),
            porVencer60Dias = await _db.LicenciasTecnicas.CountAsync(x => x.FechaVencimiento >= hoy && x.FechaVencimiento <= limite && x.Estado != "CANCELADA"),
            activas = await _db.LicenciasTecnicas.CountAsync(x => x.Estado == "ACTIVA"),
            canceladas = await _db.LicenciasTecnicas.CountAsync(x => x.Estado == "CANCELADA")
        };

        return Ok(data);
    }
}
