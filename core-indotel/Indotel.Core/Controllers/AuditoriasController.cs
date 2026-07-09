using System.Security.Claims;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,Auditor")]
[ApiController]
[Route("api/auditorias")]
public class AuditoriasController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public AuditoriasController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entidad,
        [FromQuery] string? accion,
        [FromQuery] int? usuarioId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.Auditorias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entidad))
        {
            query = query.Where(x => x.Entidad == entidad.Trim());
        }

        if (!string.IsNullOrWhiteSpace(accion))
        {
            query = query.Where(x => x.Accion == accion.Trim());
        }

        if (usuarioId is not null)
        {
            query = query.Where(x => x.UsuarioId == usuarioId.Value);
        }

        if (desde is not null)
        {
            query = query.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta is not null)
        {
            query = query.Where(x => x.Fecha <= hasta.Value);
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var auditoria = await _db.Auditorias.FindAsync(id);
        return auditoria is null ? NotFound(new { mensaje = "Auditoria no encontrada" }) : Ok(auditoria);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create(AuditoriaCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Entidad))
        {
            return BadRequest(new { mensaje = "La entidad es obligatoria" });
        }

        if (string.IsNullOrWhiteSpace(request.Accion))
        {
            return BadRequest(new { mensaje = "La accion es obligatoria" });
        }

        var auditoria = CrearAuditoriaBase(
            request.Entidad.Trim(),
            request.EntidadId.Trim(),
            request.Accion.Trim(),
            request.Nivel.Trim(),
            request.Detalle.Trim(),
            request.EstadoAnterior.Trim(),
            request.EstadoNuevo.Trim());

        _db.Auditorias.Add(auditoria);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = auditoria.Id }, auditoria);
    }

    private Auditoria CrearAuditoriaBase(
        string entidad,
        string entidadId,
        string accion,
        string nivel,
        string detalle,
        string estadoAnterior,
        string estadoNuevo)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);

        return new Auditoria
        {
            UsuarioId = userId == 0 ? null : userId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            Nivel = string.IsNullOrWhiteSpace(nivel) ? "INFO" : nivel.ToUpperInvariant(),
            Detalle = detalle,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            MetodoHttp = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier,
            Fecha = DateTime.UtcNow
        };
    }
}
