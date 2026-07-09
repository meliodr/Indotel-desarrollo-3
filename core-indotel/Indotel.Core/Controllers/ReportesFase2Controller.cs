using Indotel.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
[ApiController]
[Route("api/reportes")]
public class ReportesFase2Controller : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ReportesFase2Controller(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("autorizaciones")]
    public async Task<IActionResult> Autorizaciones()
    {
        var porEstado = await _db.SolicitudesAutorizacion
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var data = new
        {
            total = await _db.SolicitudesAutorizacion.CountAsync(),
            recibidas = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "RECIBIDA"),
            enRevision = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "EN_REVISION"),
            aprobadas = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "APROBADA"),
            rechazadas = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "RECHAZADA"),
            vencidas = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "VENCIDA"),
            renovadas = await _db.SolicitudesAutorizacion.CountAsync(x => x.Estado == "RENOVADA"),
            porEstado
        };

        return Ok(data);
    }

    [HttpGet("certificaciones")]
    public async Task<IActionResult> Certificaciones()
    {
        var porEstado = await _db.SolicitudesCertificacion
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var data = new
        {
            total = await _db.SolicitudesCertificacion.CountAsync(),
            recibidas = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "RECIBIDA"),
            enRevision = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "EN_REVISION"),
            aprobadas = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "APROBADA"),
            rechazadas = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "RECHAZADA"),
            vencidas = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "VENCIDA"),
            renovadas = await _db.SolicitudesCertificacion.CountAsync(x => x.Estado == "RENOVADA"),
            porEstado
        };

        return Ok(data);
    }

    [HttpGet("espectro")]
    public async Task<IActionResult> Espectro()
    {
        var porEstado = await _db.FrecuenciasRadioelectricas
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var data = new
        {
            totalFrecuencias = await _db.FrecuenciasRadioelectricas.CountAsync(),
            disponibles = await _db.FrecuenciasRadioelectricas.CountAsync(x => x.Estado == "DISPONIBLE"),
            asignadas = await _db.FrecuenciasRadioelectricas.CountAsync(x => x.Estado == "ASIGNADA"),
            reservadas = await _db.FrecuenciasRadioelectricas.CountAsync(x => x.Estado == "RESERVADA"),
            suspendidas = await _db.FrecuenciasRadioelectricas.CountAsync(x => x.Estado == "SUSPENDIDA"),
            asignacionesActivas = await _db.AsignacionesFrecuencia.CountAsync(x => x.Activa),
            porEstado
        };

        return Ok(data);
    }

    [HttpGet("licencias-tecnicas")]
    public async Task<IActionResult> LicenciasTecnicas()
    {
        var limite = DateTime.UtcNow.AddDays(60);
        var porEstado = await _db.LicenciasTecnicas
            .GroupBy(x => x.Estado)
            .Select(x => new { estado = x.Key, total = x.Count() })
            .OrderByDescending(x => x.total)
            .ToListAsync();

        var data = new
        {
            total = await _db.LicenciasTecnicas.CountAsync(),
            activas = await _db.LicenciasTecnicas.CountAsync(x => x.Estado == "ACTIVA"),
            porVencer = await _db.LicenciasTecnicas.CountAsync(x => x.FechaVencimiento <= limite && x.Estado != "CANCELADA"),
            vencidas = await _db.LicenciasTecnicas.CountAsync(x => x.Estado == "VENCIDA"),
            canceladas = await _db.LicenciasTecnicas.CountAsync(x => x.Estado == "CANCELADA"),
            porEstado
        };

        return Ok(data);
    }
}
