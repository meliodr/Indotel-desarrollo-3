using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/prestadoras")]
public class PrestadorasController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public PrestadorasController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.Prestadoras
            .Where(x => x.Activa)
            .OrderBy(x => x.NombreComercial)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var prestadora = await _db.Prestadoras.FindAsync(id);
        return prestadora is null ? NotFound(new { mensaje = "Prestadora no encontrada" }) : Ok(prestadora);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(PrestadoraCreateDto request)
    {
        var validacion = ValidarPrestadora(request.Rnc, request.NombreComercial, request.RazonSocial, request.Correo);
        if (validacion is not null)
        {
            return BadRequest(new { mensaje = validacion });
        }

        var rnc = request.Rnc.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();

        var existeRnc = await _db.Prestadoras.AnyAsync(x => x.Rnc == rnc);
        if (existeRnc)
        {
            return Conflict(new { mensaje = "Ya existe una prestadora con este RNC" });
        }

        var existeCorreo = await _db.Prestadoras.AnyAsync(x => x.Correo == correo);
        if (existeCorreo)
        {
            return Conflict(new { mensaje = "Ya existe una prestadora con este correo" });
        }

        var prestadora = new Prestadora
        {
            Rnc = rnc,
            NombreComercial = request.NombreComercial.Trim(),
            RazonSocial = request.RazonSocial.Trim(),
            Representante = request.Representante.Trim(),
            Telefono = request.Telefono.Trim(),
            Correo = correo,
            Activa = true
        };

        _db.Prestadoras.Add(prestadora);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = prestadora.Id }, prestadora);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PrestadoraUpdateDto request)
    {
        var prestadora = await _db.Prestadoras.FindAsync(id);
        if (prestadora is null) return NotFound(new { mensaje = "Prestadora no encontrada" });

        var validacion = ValidarPrestadora(request.Rnc, request.NombreComercial, request.RazonSocial, request.Correo);
        if (validacion is not null)
        {
            return BadRequest(new { mensaje = validacion });
        }

        var rnc = request.Rnc.Trim();
        var correo = request.Correo.Trim().ToLowerInvariant();

        var existeRnc = await _db.Prestadoras.AnyAsync(x => x.Id != id && x.Rnc == rnc);
        if (existeRnc)
        {
            return Conflict(new { mensaje = "Ya existe otra prestadora con este RNC" });
        }

        var existeCorreo = await _db.Prestadoras.AnyAsync(x => x.Id != id && x.Correo == correo);
        if (existeCorreo)
        {
            return Conflict(new { mensaje = "Ya existe otra prestadora con este correo" });
        }

        prestadora.Rnc = rnc;
        prestadora.NombreComercial = request.NombreComercial.Trim();
        prestadora.RazonSocial = request.RazonSocial.Trim();
        prestadora.Representante = request.Representante.Trim();
        prestadora.Telefono = request.Telefono.Trim();
        prestadora.Correo = correo;

        await _db.SaveChangesAsync();

        return Ok(prestadora);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, PrestadoraEstadoDto request)
    {
        var prestadora = await _db.Prestadoras.FindAsync(id);
        if (prestadora is null) return NotFound(new { mensaje = "Prestadora no encontrada" });

        prestadora.Activa = request.Activa;
        await _db.SaveChangesAsync();

        return Ok(prestadora);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora")]
    [HttpGet("{id:int}/reclamaciones")]
    public async Task<IActionResult> GetReclamaciones(int id)
    {
        var prestadora = await _db.Prestadoras.FindAsync(id);
        if (prestadora is null) return NotFound(new { mensaje = "Prestadora no encontrada" });

        var data = await _db.Reclamaciones
            .Where(x => x.PrestadoraId == id)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(data);
    }

    private static string? ValidarPrestadora(string rnc, string nombreComercial, string razonSocial, string correo)
    {
        if (string.IsNullOrWhiteSpace(rnc)) return "El RNC es obligatorio";
        if (string.IsNullOrWhiteSpace(nombreComercial)) return "El nombre comercial es obligatorio";
        if (string.IsNullOrWhiteSpace(razonSocial)) return "La razon social es obligatoria";
        if (string.IsNullOrWhiteSpace(correo)) return "El correo es obligatorio";
        if (!correo.Contains('@')) return "El correo no tiene un formato valido";

        return null;
    }
}
