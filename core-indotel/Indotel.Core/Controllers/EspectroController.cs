using System.Security.Claims;
using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor")]
[ApiController]
[Route("api/espectro")]
public class EspectroController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public EspectroController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet("frecuencias")]
    public async Task<IActionResult> GetFrecuencias([FromQuery] string? estado, [FromQuery] string? region)
    {
        var query = _db.FrecuenciasRadioelectricas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(x => x.Estado == FrecuenciaEstados.Normalizar(estado));
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            query = query.Where(x => x.Region == region.Trim());
        }

        var data = await query.OrderBy(x => x.RangoInicioMHz).ToListAsync();
        return Ok(data);
    }

    [HttpGet("frecuencias/{id:int}")]
    public async Task<IActionResult> GetFrecuenciaById(int id)
    {
        var frecuencia = await _db.FrecuenciasRadioelectricas.FindAsync(id);
        return frecuencia is null ? NotFound(new { mensaje = "Frecuencia no encontrada" }) : Ok(frecuencia);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("frecuencias")]
    public async Task<IActionResult> CreateFrecuencia(FrecuenciaRadioelectricaCreateDto request)
    {
        if (request.RangoInicioMHz <= 0 || request.RangoFinMHz <= 0 || request.RangoFinMHz <= request.RangoInicioMHz)
        {
            return BadRequest(new { mensaje = "El rango de frecuencia no es valido" });
        }

        var region = request.Region.Trim();
        var existe = await _db.FrecuenciasRadioelectricas.AnyAsync(x =>
            x.RangoInicioMHz == request.RangoInicioMHz &&
            x.RangoFinMHz == request.RangoFinMHz &&
            x.Region == region);

        if (existe)
        {
            return Conflict(new { mensaje = "Ya existe este rango de frecuencia para la region indicada" });
        }

        var frecuencia = new FrecuenciaRadioelectrica
        {
            RangoInicioMHz = request.RangoInicioMHz,
            RangoFinMHz = request.RangoFinMHz,
            Banda = request.Banda.Trim(),
            ServicioUso = request.ServicioUso.Trim(),
            Provincia = request.Provincia.Trim(),
            Region = region,
            Estado = FrecuenciaEstados.Disponible,
            Observacion = request.Observacion.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _db.FrecuenciasRadioelectricas.Add(frecuencia);
        await _db.SaveChangesAsync();

        await RegistrarAuditoria("FrecuenciaRadioelectrica", frecuencia.Id.ToString(), "CREAR_FRECUENCIA", "INFO", "Frecuencia radioelectrica registrada", string.Empty, frecuencia.Estado);

        return CreatedAtAction(nameof(GetFrecuenciaById), new { id = frecuencia.Id }, frecuencia);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("frecuencias/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoFrecuencia(int id, CambiarEstadoFrecuenciaDto request)
    {
        var frecuencia = await _db.FrecuenciasRadioelectricas.FindAsync(id);
        if (frecuencia is null) return NotFound(new { mensaje = "Frecuencia no encontrada" });

        var estadoAnterior = FrecuenciaEstados.Normalizar(frecuencia.Estado);
        var estadoNuevo = FrecuenciaEstados.Normalizar(request.EstadoNuevo);

        if (!FrecuenciaEstados.Existe(estadoNuevo))
        {
            return BadRequest(new { mensaje = "Estado no permitido" });
        }

        frecuencia.Estado = estadoNuevo;
        frecuencia.Observacion = request.Observacion.Trim();
        frecuencia.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria("FrecuenciaRadioelectrica", frecuencia.Id.ToString(), "CAMBIO_ESTADO_FRECUENCIA", "INFO", request.Observacion.Trim(), estadoAnterior, estadoNuevo);

        return Ok(frecuencia);
    }

    [HttpGet("asignaciones")]
    public async Task<IActionResult> GetAsignaciones([FromQuery] bool soloActivas = true)
    {
        var query = _db.AsignacionesFrecuencia
            .Include(x => x.FrecuenciaRadioelectrica)
            .Include(x => x.Prestadora)
            .AsQueryable();

        if (soloActivas)
        {
            query = query.Where(x => x.Activa);
        }

        var data = await query.OrderByDescending(x => x.Id).ToListAsync();
        return Ok(data);
    }

    [HttpGet("asignaciones/{id:int}")]
    public async Task<IActionResult> GetAsignacionById(int id)
    {
        var asignacion = await _db.AsignacionesFrecuencia
            .Include(x => x.FrecuenciaRadioelectrica)
            .Include(x => x.Prestadora)
            .FirstOrDefaultAsync(x => x.Id == id);

        return asignacion is null ? NotFound(new { mensaje = "Asignacion no encontrada" }) : Ok(asignacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("asignaciones")]
    public async Task<IActionResult> CreateAsignacion(AsignacionFrecuenciaCreateDto request)
    {
        var frecuencia = await _db.FrecuenciasRadioelectricas.FindAsync(request.FrecuenciaRadioelectricaId);
        if (frecuencia is null) return BadRequest(new { mensaje = "Frecuencia no encontrada" });

        if (frecuencia.Estado == FrecuenciaEstados.Asignada)
        {
            return Conflict(new { mensaje = "La frecuencia ya esta asignada" });
        }

        var existeAsignacionActiva = await _db.AsignacionesFrecuencia.AnyAsync(x => x.FrecuenciaRadioelectricaId == frecuencia.Id && x.Activa);
        if (existeAsignacionActiva)
        {
            return Conflict(new { mensaje = "Ya existe una asignacion activa para esta frecuencia" });
        }

        if (request.PrestadoraId is not null)
        {
            var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId.Value);
            if (!prestadoraExiste) return BadRequest(new { mensaje = "Prestadora no encontrada" });
        }

        var asignacion = new AsignacionFrecuencia
        {
            FrecuenciaRadioelectricaId = frecuencia.Id,
            PrestadoraId = request.PrestadoraId,
            EntidadAsignada = request.EntidadAsignada.Trim(),
            UsoAutorizado = request.UsoAutorizado.Trim(),
            Provincia = request.Provincia.Trim(),
            Region = request.Region.Trim(),
            FechaAsignacion = DateTime.UtcNow,
            FechaFin = request.FechaFin,
            Activa = true,
            Observacion = request.Observacion.Trim(),
            UsuarioAsignacionId = ObtenerUsuarioIdActual()
        };

        frecuencia.Estado = FrecuenciaEstados.Asignada;
        frecuencia.FechaActualizacion = DateTime.UtcNow;

        _db.AsignacionesFrecuencia.Add(asignacion);
        await _db.SaveChangesAsync();

        await RegistrarAuditoria("AsignacionFrecuencia", asignacion.Id.ToString(), "ASIGNAR_FRECUENCIA", "INFO", "Frecuencia asignada", FrecuenciaEstados.Disponible, FrecuenciaEstados.Asignada);

        return CreatedAtAction(nameof(GetAsignacionById), new { id = asignacion.Id }, asignacion);
    }

    private int ObtenerUsuarioIdActual()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);
        return userId;
    }

    private async Task RegistrarAuditoria(string entidad, string entidadId, string accion, string nivel, string detalle, string estadoAnterior, string estadoNuevo)
    {
        var userId = ObtenerUsuarioIdActual();
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = userId == 0 ? null : userId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            Nivel = string.IsNullOrWhiteSpace(nivel) ? "INFO" : nivel.ToUpperInvariant(),
            Detalle = detalle ?? string.Empty,
            EstadoAnterior = estadoAnterior ?? string.Empty,
            EstadoNuevo = estadoNuevo ?? string.Empty,
            MetodoHttp = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier,
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
