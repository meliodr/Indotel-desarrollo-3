using System.Security.Claims;
using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
[ApiController]
[Route("api/autorizaciones")]
public class AutorizacionesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public AutorizacionesController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.SolicitudesAutorizacion
            .Include(x => x.TipoAutorizacion)
            .AsQueryable();

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            if (prestadoraId is null) return Forbid();
            query = query.Where(x => x.PrestadoraId == prestadoraId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(x => x.Estado == SolicitudInstitucionalEstados.Normalizar(estado));
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var solicitud = await _db.SolicitudesAutorizacion
            .Include(x => x.TipoAutorizacion)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (solicitud is null) return NotFound(new { mensaje = "Solicitud de autorizacion no encontrada" });
        if (!await PuedeVerSolicitud(solicitud)) return Forbid();

        return Ok(solicitud);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(SolicitudAutorizacionCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SolicitanteNombre))
        {
            return BadRequest(new { mensaje = "El solicitante es obligatorio" });
        }

        if (string.IsNullOrWhiteSpace(request.SolicitanteRnc))
        {
            return BadRequest(new { mensaje = "El RNC del solicitante es obligatorio" });
        }

        var tipoExiste = await _db.TiposAutorizacion.AnyAsync(x => x.Id == request.TipoAutorizacionId && x.Activo);
        if (!tipoExiste)
        {
            return BadRequest(new { mensaje = "Tipo de autorizacion no valido" });
        }

        if (request.PrestadoraId is not null)
        {
            var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId.Value);
            if (!prestadoraExiste) return BadRequest(new { mensaje = "Prestadora no encontrada" });
        }

        var solicitud = new SolicitudAutorizacion
        {
            NumeroSolicitud = await GenerarNumeroSolicitud(),
            SolicitanteNombre = request.SolicitanteNombre.Trim(),
            SolicitanteRnc = request.SolicitanteRnc.Trim(),
            PrestadoraId = request.PrestadoraId,
            TipoAutorizacionId = request.TipoAutorizacionId,
            Estado = SolicitudInstitucionalEstados.Recibida,
            Descripcion = request.Descripcion.Trim(),
            FechaSolicitud = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow,
            UsuarioResponsableId = ObtenerUsuarioIdActual()
        };

        _db.SolicitudesAutorizacion.Add(solicitud);
        await _db.SaveChangesAsync();

        await RegistrarAuditoria(solicitud.Id.ToString(), "CREAR_SOLICITUD_AUTORIZACION", "INFO", $"Solicitud de autorizacion creada: {solicitud.NumeroSolicitud}", string.Empty, solicitud.Estado);

        return CreatedAtAction(nameof(GetById), new { id = solicitud.Id }, solicitud);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoSolicitudInstitucionalDto request)
    {
        var solicitud = await _db.SolicitudesAutorizacion.FindAsync(id);
        if (solicitud is null) return NotFound(new { mensaje = "Solicitud de autorizacion no encontrada" });

        var estadoAnterior = SolicitudInstitucionalEstados.Normalizar(solicitud.Estado);
        var estadoNuevo = SolicitudInstitucionalEstados.Normalizar(request.EstadoNuevo);

        if (!SolicitudInstitucionalEstados.Existe(estadoNuevo))
        {
            return BadRequest(new { mensaje = "Estado no permitido" });
        }

        if (!SolicitudInstitucionalEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Transicion de estado no permitida", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        if (request.ResolucionInstitucionalId is not null)
        {
            var resolucionExiste = await _db.ResolucionesInstitucionales.AnyAsync(x => x.Id == request.ResolucionInstitucionalId.Value);
            if (!resolucionExiste) return BadRequest(new { mensaje = "Resolucion institucional no encontrada" });
            solicitud.ResolucionInstitucionalId = request.ResolucionInstitucionalId;
        }

        solicitud.Estado = estadoNuevo;
        solicitud.ComentarioRevision = request.Comentario.Trim();
        solicitud.UsuarioResponsableId = ObtenerUsuarioIdActual();
        solicitud.FechaActualizacion = DateTime.UtcNow;

        if (estadoNuevo == SolicitudInstitucionalEstados.EnRevision)
        {
            solicitud.FechaRevision = DateTime.UtcNow;
        }
        else if (estadoNuevo == SolicitudInstitucionalEstados.Aprobada)
        {
            solicitud.FechaAprobacion = DateTime.UtcNow;
            solicitud.FechaVencimiento = request.FechaVencimiento ?? DateTime.UtcNow.AddYears(1);
        }
        else if (estadoNuevo == SolicitudInstitucionalEstados.Rechazada)
        {
            solicitud.FechaRechazo = DateTime.UtcNow;
            solicitud.MotivoRechazo = request.Comentario.Trim();
        }

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(solicitud.Id.ToString(), "CAMBIO_ESTADO_AUTORIZACION", "INFO", request.Comentario.Trim(), estadoAnterior, estadoNuevo);

        return Ok(solicitud);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("{id:int}/renovar")]
    public async Task<IActionResult> Renovar(int id, RenovarSolicitudInstitucionalDto request)
    {
        var solicitud = await _db.SolicitudesAutorizacion.FindAsync(id);
        if (solicitud is null) return NotFound(new { mensaje = "Solicitud de autorizacion no encontrada" });

        var estadoAnterior = SolicitudInstitucionalEstados.Normalizar(solicitud.Estado);
        var estadoNuevo = SolicitudInstitucionalEstados.Renovada;

        if (!SolicitudInstitucionalEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Solo se puede renovar una autorizacion aprobada o vencida", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        solicitud.Estado = estadoNuevo;
        solicitud.FechaRenovacion = DateTime.UtcNow;
        solicitud.FechaVencimiento = request.NuevaFechaVencimiento ?? DateTime.UtcNow.AddYears(1);
        solicitud.ComentarioRevision = request.Comentario.Trim();
        solicitud.UsuarioResponsableId = ObtenerUsuarioIdActual();
        solicitud.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(solicitud.Id.ToString(), "RENOVAR_AUTORIZACION", "INFO", request.Comentario.Trim(), estadoAnterior, estadoNuevo);

        return Ok(solicitud);
    }

    private async Task<bool> PuedeVerSolicitud(SolicitudAutorizacion solicitud)
    {
        if (User.IsInRole("Administrador") || User.IsInRole("AnalistaDAU") || User.IsInRole("Auditor")) return true;

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            return prestadoraId is not null && solicitud.PrestadoraId == prestadoraId.Value;
        }

        return false;
    }

    private async Task<string> GenerarNumeroSolicitud()
    {
        for (var intento = 0; intento < 10; intento++)
        {
            var numero = $"AUT-IND-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(100, 999)}";
            var existe = await _db.SolicitudesAutorizacion.AnyAsync(x => x.NumeroSolicitud == numero);
            if (!existe) return numero;
        }

        return $"AUT-IND-{DateTime.UtcNow:yyyyMMddHHmmssfffffff}-{Guid.NewGuid():N}";
    }

    private async Task<int?> ObtenerPrestadoraIdActual()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(correo)) return null;

        return await _db.Prestadoras
            .Where(x => x.Correo == correo && x.Activa)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private int ObtenerUsuarioIdActual()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);
        return userId;
    }

    private async Task RegistrarAuditoria(string entidadId, string accion, string nivel, string detalle, string estadoAnterior, string estadoNuevo)
    {
        var userId = ObtenerUsuarioIdActual();
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = userId == 0 ? null : userId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = "SolicitudAutorizacion",
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
