using System.Security.Claims;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/notificaciones")]
public class NotificacionesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public NotificacionesController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloNoLeidas = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.Notificaciones.AsQueryable();

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoId = await ObtenerCiudadanoIdActual();
            if (ciudadanoId is null) return Forbid();
            query = query.Where(x => x.CiudadanoId == ciudadanoId.Value);
        }
        else if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            if (prestadoraId is null) return Forbid();
            query = query.Where(x => x.PrestadoraId == prestadoraId.Value);
        }

        if (soloNoLeidas)
        {
            query = query.Where(x => x.FechaLectura == null);
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, data });
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notificacion = await _db.Notificaciones.FindAsync(id);
        if (notificacion is null) return NotFound(new { mensaje = "Notificacion no encontrada" });

        if (!await PuedeVerNotificacion(notificacion)) return Forbid();

        return Ok(notificacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(NotificacionCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Asunto))
        {
            return BadRequest(new { mensaje = "El asunto es obligatorio" });
        }

        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            return BadRequest(new { mensaje = "El mensaje es obligatorio" });
        }

        var notificacion = new Notificacion
        {
            UsuarioId = request.UsuarioId,
            CiudadanoId = request.CiudadanoId,
            PrestadoraId = request.PrestadoraId,
            ReclamacionId = request.ReclamacionId,
            Canal = string.IsNullOrWhiteSpace(request.Canal) ? "INTERNA" : request.Canal.Trim().ToUpperInvariant(),
            Destinatario = request.Destinatario.Trim(),
            Asunto = request.Asunto.Trim(),
            Mensaje = request.Mensaje.Trim(),
            Estado = "PENDIENTE",
            FechaCreacion = DateTime.UtcNow
        };

        _db.Notificaciones.Add(notificacion);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = notificacion.Id }, notificacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Prestadora,Ciudadano")]
    [HttpPatch("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        var notificacion = await _db.Notificaciones.FindAsync(id);
        if (notificacion is null) return NotFound(new { mensaje = "Notificacion no encontrada" });

        if (!await PuedeVerNotificacion(notificacion)) return Forbid();

        notificacion.FechaLectura ??= DateTime.UtcNow;
        notificacion.Estado = "LEIDA";
        await _db.SaveChangesAsync();

        return Ok(notificacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/enviar")]
    public async Task<IActionResult> MarcarEnviada(int id)
    {
        var notificacion = await _db.Notificaciones.FindAsync(id);
        if (notificacion is null) return NotFound(new { mensaje = "Notificacion no encontrada" });

        notificacion.FechaEnvio ??= DateTime.UtcNow;
        notificacion.Estado = "ENVIADA";
        await _db.SaveChangesAsync();

        return Ok(notificacion);
    }

    private async Task<bool> PuedeVerNotificacion(Notificacion notificacion)
    {
        if (User.IsInRole("Administrador") || User.IsInRole("AnalistaDAU") || User.IsInRole("Auditor"))
        {
            return true;
        }

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoId = await ObtenerCiudadanoIdActual();
            return ciudadanoId is not null && notificacion.CiudadanoId == ciudadanoId.Value;
        }

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            return prestadoraId is not null && notificacion.PrestadoraId == prestadoraId.Value;
        }

        return false;
    }

    private async Task<int?> ObtenerCiudadanoIdActual()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(correo)) return null;

        return await _db.Ciudadanos
            .Where(x => x.Correo == correo && x.Activo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
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
}
