using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/ciudadanos")]
public class CiudadanosController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public CiudadanosController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.Ciudadanos.OrderByDescending(x => x.Id).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ciudadano = await _db.Ciudadanos.FindAsync(id);
        return ciudadano is null ? NotFound() : Ok(ciudadano);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CiudadanoCreateDto request)
    {
        if (await _db.Ciudadanos.AnyAsync(x => x.Cedula == request.Cedula))
        {
            return Conflict(new { mensaje = "Ya existe un ciudadano con esta cedula" });
        }

        var ciudadano = new Ciudadano
        {
            Cedula = request.Cedula,
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Telefono = request.Telefono,
            Correo = request.Correo,
            Direccion = request.Direccion
        };

        _db.Ciudadanos.Add(ciudadano);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ciudadano.Id }, ciudadano);
    }
}
