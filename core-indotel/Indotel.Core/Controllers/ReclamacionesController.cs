using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api/reclamaciones")]
public class ReclamacionesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ReclamacionesController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _db.Reclamaciones.OrderByDescending(x => x.Id).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        return reclamacion is null ? NotFound() : Ok(reclamacion);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReclamacionCreateDto request)
    {
        var ciudadanoExiste = await _db.Ciudadanos.AnyAsync(x => x.Id == request.CiudadanoId);
        var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId);
        var servicioExiste = await _db.ServiciosTelecom.AnyAsync(x => x.Id == request.ServicioTelecomId);

        if (!ciudadanoExiste || !prestadoraExiste || !servicioExiste)
        {
            return BadRequest(new { mensaje = "Ciudadano, prestadora o servicio no valido" });
        }

        var reclamacion = new Reclamacion
        {
            NumeroExpediente = $"IND-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CiudadanoId = request.CiudadanoId,
            PrestadoraId = request.PrestadoraId,
            ServicioTelecomId = request.ServicioTelecomId,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Estado = "RECIBIDA"
        };

        _db.Reclamaciones.Add(reclamacion);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = reclamacion.Id }, reclamacion);
    }
}
