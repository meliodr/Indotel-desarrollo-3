using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/catalogos/reclamaciones")]
public class ClasificacionReclamacionesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ClasificacionReclamacionesController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos()
    {
        var data = await _db.TiposReclamacion
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("motivos")]
    public async Task<IActionResult> GetMotivos([FromQuery] int? tipoReclamacionId)
    {
        var query = _db.MotivosReclamacion
            .Include(x => x.TipoReclamacion)
            .Where(x => x.Activo)
            .AsQueryable();

        if (tipoReclamacionId is not null)
        {
            query = query.Where(x => x.TipoReclamacionId == tipoReclamacionId.Value);
        }

        var data = await query
            .OrderBy(x => x.TipoReclamacionId)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("tipos")]
    public async Task<IActionResult> CreateTipo(TipoReclamacionCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del tipo es obligatorio" });
        }

        var nombre = request.Nombre.Trim();
        var existe = await _db.TiposReclamacion.AnyAsync(x => x.Nombre == nombre);

        if (existe)
        {
            return Conflict(new { mensaje = "Ya existe un tipo de reclamacion con este nombre" });
        }

        var tipo = new TipoReclamacion
        {
            Nombre = nombre,
            Descripcion = request.Descripcion.Trim(),
            Activo = true
        };

        _db.TiposReclamacion.Add(tipo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTipos), tipo);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("motivos")]
    public async Task<IActionResult> CreateMotivo(MotivoReclamacionCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { mensaje = "El nombre del motivo es obligatorio" });
        }

        var tipoExiste = await _db.TiposReclamacion.AnyAsync(x => x.Id == request.TipoReclamacionId && x.Activo);
        if (!tipoExiste)
        {
            return BadRequest(new { mensaje = "El tipo de reclamacion no existe o esta inactivo" });
        }

        var nombre = request.Nombre.Trim();
        var existe = await _db.MotivosReclamacion.AnyAsync(x => x.TipoReclamacionId == request.TipoReclamacionId && x.Nombre == nombre);

        if (existe)
        {
            return Conflict(new { mensaje = "Ya existe un motivo con este nombre para este tipo" });
        }

        var motivo = new MotivoReclamacion
        {
            TipoReclamacionId = request.TipoReclamacionId,
            Nombre = nombre,
            Descripcion = request.Descripcion.Trim(),
            Activo = true
        };

        _db.MotivosReclamacion.Add(motivo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMotivos), motivo);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("tipos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoTipo(int id, ClasificacionEstadoDto request)
    {
        var tipo = await _db.TiposReclamacion.FindAsync(id);
        if (tipo is null) return NotFound(new { mensaje = "Tipo de reclamacion no encontrado" });

        tipo.Activo = request.Activo;
        await _db.SaveChangesAsync();

        return Ok(tipo);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("motivos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoMotivo(int id, ClasificacionEstadoDto request)
    {
        var motivo = await _db.MotivosReclamacion.FindAsync(id);
        if (motivo is null) return NotFound(new { mensaje = "Motivo de reclamacion no encontrado" });

        motivo.Activo = request.Activo;
        await _db.SaveChangesAsync();

        return Ok(motivo);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("canales")]
    public IActionResult GetCanales()
    {
        return Ok(ReclamacionClasificacion.CanalesRecepcion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("prioridades")]
    public IActionResult GetPrioridades()
    {
        return Ok(ReclamacionClasificacion.Prioridades);
    }
}
