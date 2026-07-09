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

    [HttpGet("{id:int}/historial")]
    public async Task<IActionResult> GetHistorial(int id)
    {
        var existe = await _db.Reclamaciones.AnyAsync(x => x.Id == id);
        if (!existe) return NotFound();

        var data = await _db.HistorialReclamaciones
            .Where(x => x.ReclamacionId == id)
            .OrderByDescending(x => x.FechaCambio)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}/respuestas")]
    public async Task<IActionResult> GetRespuestas(int id)
    {
        var existe = await _db.Reclamaciones.AnyAsync(x => x.Id == id);
        if (!existe) return NotFound();

        var data = await _db.RespuestasPrestadora
            .Where(x => x.ReclamacionId == id)
            .OrderByDescending(x => x.FechaRespuesta)
            .ToListAsync();

        return Ok(data);
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

        await RegistrarHistorial(reclamacion.Id, string.Empty, "RECIBIDA", "Reclamacion registrada");

        return CreatedAtAction(nameof(GetById), new { id = reclamacion.Id }, reclamacion);
    }

    [HttpPut("{id:int}/estado")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoReclamacionDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        var estadosPermitidos = new[] { "RECIBIDA", "VALIDADA", "ENVIADA_A_PRESTADORA", "RESPONDIDA_POR_PRESTADORA", "EN_REVISION", "RESUELTA", "CERRADA", "RECHAZADA" };
        if (!estadosPermitidos.Contains(request.EstadoNuevo))
        {
            return BadRequest(new { mensaje = "Estado no permitido" });
        }

        var estadoAnterior = reclamacion.Estado;
        reclamacion.Estado = request.EstadoNuevo;

        if (request.EstadoNuevo == "CERRADA" || request.EstadoNuevo == "RESUELTA")
        {
            reclamacion.FechaCierre = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, request.EstadoNuevo, request.Comentario);

        return Ok(reclamacion);
    }

    [HttpPost("{id:int}/respuesta-prestadora")]
    public async Task<IActionResult> RegistrarRespuestaPrestadora(int id, RespuestaPrestadoraCreateDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (reclamacion.PrestadoraId != request.PrestadoraId)
        {
            return BadRequest(new { mensaje = "La prestadora no corresponde a esta reclamacion" });
        }

        var respuesta = new RespuestaPrestadora
        {
            ReclamacionId = id,
            PrestadoraId = request.PrestadoraId,
            Respuesta = request.Respuesta,
            DocumentoSoporte = request.DocumentoSoporte
        };

        _db.RespuestasPrestadora.Add(respuesta);

        var estadoAnterior = reclamacion.Estado;
        reclamacion.Estado = "RESPONDIDA_POR_PRESTADORA";

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, "RESPONDIDA_POR_PRESTADORA", "Respuesta registrada por la prestadora");

        return Ok(respuesta);
    }

    private async Task RegistrarHistorial(int reclamacionId, string estadoAnterior, string estadoNuevo, string comentario)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);

        _db.HistorialReclamaciones.Add(new HistorialReclamacion
        {
            ReclamacionId = reclamacionId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            Comentario = comentario,
            UsuarioId = userId,
            FechaCambio = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
