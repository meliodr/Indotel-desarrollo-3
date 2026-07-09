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

    [HttpGet("cedula/{cedula}")]
    public async Task<IActionResult> GetByCedula(string cedula)
    {
        var ciudadano = await _db.Ciudadanos.FirstOrDefaultAsync(x => x.Cedula == cedula);
        return ciudadano is null ? NotFound(new { mensaje = "No existe un ciudadano con esta cedula" }) : Ok(ciudadano);
    }

    [HttpGet("{id:int}/reclamaciones")]
    public async Task<IActionResult> GetReclamacionesByCiudadano(int id)
    {
        var ciudadanoExiste = await _db.Ciudadanos.AnyAsync(x => x.Id == id);
        if (!ciudadanoExiste) return NotFound(new { mensaje = "Ciudadano no encontrado" });

        var data = await _db.Reclamaciones
            .Where(x => x.CiudadanoId == id)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(data);
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CiudadanoUpdateDto request)
    {
        var ciudadano = await _db.Ciudadanos.FindAsync(id);
        if (ciudadano is null) return NotFound(new { mensaje = "Ciudadano no encontrado" });

        ciudadano.Nombres = request.Nombres;
        ciudadano.Apellidos = request.Apellidos;
        ciudadano.Telefono = request.Telefono;
        ciudadano.Correo = request.Correo;
        ciudadano.Direccion = request.Direccion;

        await _db.SaveChangesAsync();

        return Ok(ciudadano);
    }
}
