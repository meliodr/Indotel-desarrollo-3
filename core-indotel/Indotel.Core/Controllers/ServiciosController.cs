using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/servicios")]
public class ServiciosController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ServiciosController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.ServiciosTelecom
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var servicio = await _db.ServiciosTelecom.FindAsync(id);
        return servicio is null ? NotFound(new { mensaje = "Servicio no encontrado" }) : Ok(servicio);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(ServicioCreateDto request)
    {
        var validacion = ValidarServicio(request.Nombre);
        if (validacion is not null)
        {
            return BadRequest(new { mensaje = validacion });
        }

        var nombre = request.Nombre.Trim();

        var existeNombre = await _db.ServiciosTelecom.AnyAsync(x => x.Nombre == nombre);
        if (existeNombre)
        {
            return Conflict(new { mensaje = "Ya existe un servicio con este nombre" });
        }

        var servicio = new ServicioTelecom
        {
            Nombre = nombre,
            Descripcion = request.Descripcion.Trim(),
            Activo = true
        };

        _db.ServiciosTelecom.Add(servicio);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = servicio.Id }, servicio);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ServicioUpdateDto request)
    {
        var servicio = await _db.ServiciosTelecom.FindAsync(id);
        if (servicio is null) return NotFound(new { mensaje = "Servicio no encontrado" });

        var validacion = ValidarServicio(request.Nombre);
        if (validacion is not null)
        {
            return BadRequest(new { mensaje = validacion });
        }

        var nombre = request.Nombre.Trim();

        var existeNombre = await _db.ServiciosTelecom.AnyAsync(x => x.Id != id && x.Nombre == nombre);
        if (existeNombre)
        {
            return Conflict(new { mensaje = "Ya existe otro servicio con este nombre" });
        }

        servicio.Nombre = nombre;
        servicio.Descripcion = request.Descripcion.Trim();

        await _db.SaveChangesAsync();

        return Ok(servicio);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, ServicioEstadoDto request)
    {
        var servicio = await _db.ServiciosTelecom.FindAsync(id);
        if (servicio is null) return NotFound(new { mensaje = "Servicio no encontrado" });

        servicio.Activo = request.Activo;
        await _db.SaveChangesAsync();

        return Ok(servicio);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor")]
    [HttpGet("{id:int}/reclamaciones")]
    public async Task<IActionResult> GetReclamaciones(int id)
    {
        var servicio = await _db.ServiciosTelecom.FindAsync(id);
        if (servicio is null) return NotFound(new { mensaje = "Servicio no encontrado" });

        var data = await _db.Reclamaciones
            .Where(x => x.ServicioTelecomId == id)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(data);
    }

    private static string? ValidarServicio(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return "El nombre del servicio es obligatorio";
        return null;
    }
}
