using System.Security.Claims;
using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Indotel.Core.Services;
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

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = _db.Reclamaciones.AsQueryable();

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

        var data = await query.OrderByDescending(x => x.Id).ToListAsync();
        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("expediente/{numero}")]
    public async Task<IActionResult> GetByNumeroExpediente(string numero)
    {
        var reclamacion = await _db.Reclamaciones.FirstOrDefaultAsync(x => x.NumeroExpediente == numero);
        if (reclamacion is null) return NotFound(new { mensaje = "No existe una reclamacion con este numero de expediente" });

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}/historial")]
    public async Task<IActionResult> GetHistorial(int id)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        var data = await _db.HistorialReclamaciones
            .Where(x => x.ReclamacionId == id)
            .OrderByDescending(x => x.FechaCambio)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}/respuestas")]
    public async Task<IActionResult> GetRespuestas(int id)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        var data = await _db.RespuestasPrestadora
            .Where(x => x.ReclamacionId == id)
            .OrderByDescending(x => x.FechaRespuesta)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Ciudadano")]
    [HttpPost]
    public async Task<IActionResult> Create(ReclamacionCreateDto request)
    {
        var ciudadanoId = request.CiudadanoId;

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoIdActual = await ObtenerCiudadanoIdActual();
            if (ciudadanoIdActual is null) return Forbid();

            if (request.CiudadanoId != ciudadanoIdActual.Value)
            {
                return Forbid();
            }

            ciudadanoId = ciudadanoIdActual.Value;
        }

        var ciudadanoExiste = await _db.Ciudadanos.AnyAsync(x => x.Id == ciudadanoId);
        var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId);
        var servicioExiste = await _db.ServiciosTelecom.AnyAsync(x => x.Id == request.ServicioTelecomId);

        if (!ciudadanoExiste || !prestadoraExiste || !servicioExiste)
        {
            return BadRequest(new { mensaje = "Ciudadano, prestadora o servicio no valido" });
        }

        var reclamacion = new Reclamacion
        {
            NumeroExpediente = await GenerarNumeroExpediente(),
            CiudadanoId = ciudadanoId,
            PrestadoraId = request.PrestadoraId,
            ServicioTelecomId = request.ServicioTelecomId,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Estado = ReclamacionEstados.Recibida
        };

        _db.Reclamaciones.Add(reclamacion);
        await _db.SaveChangesAsync();

        await RegistrarHistorial(reclamacion.Id, string.Empty, ReclamacionEstados.Recibida, "Reclamacion registrada");

        return CreatedAtAction(nameof(GetById), new { id = reclamacion.Id }, reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPut("{id:int}/estado")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoReclamacionDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        var estadoNuevo = ReclamacionEstadoService.NormalizarEstado(request.EstadoNuevo);
        var estadoAnterior = ReclamacionEstadoService.NormalizarEstado(reclamacion.Estado);

        if (!ReclamacionEstadoService.ExisteEstado(estadoNuevo))
        {
            return BadRequest(new { mensaje = "Estado no permitido" });
        }

        if (!ReclamacionEstadoService.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new
            {
                mensaje = ReclamacionEstadoService.CrearMensajeTransicionInvalida(estadoAnterior, estadoNuevo),
                estadoActual = estadoAnterior,
                estadoSolicitado = estadoNuevo,
                codigo = "TRANSICION_ESTADO_INVALIDA"
            });
        }

        reclamacion.Estado = estadoNuevo;

        if (estadoNuevo == ReclamacionEstados.Cerrada || estadoNuevo == ReclamacionEstados.Resuelta)
        {
            reclamacion.FechaCierre = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, estadoNuevo, request.Comentario);

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,Prestadora")]
    [HttpPost("{id:int}/respuesta-prestadora")]
    public async Task<IActionResult> RegistrarRespuestaPrestadora(int id, RespuestaPrestadoraCreateDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraIdActual = await ObtenerPrestadoraIdActual();
            if (prestadoraIdActual is null) return Forbid();

            if (request.PrestadoraId != prestadoraIdActual.Value || reclamacion.PrestadoraId != prestadoraIdActual.Value)
            {
                return Forbid();
            }
        }

        if (reclamacion.PrestadoraId != request.PrestadoraId)
        {
            return BadRequest(new { mensaje = "La prestadora no corresponde a esta reclamacion" });
        }

        var estadoAnterior = ReclamacionEstadoService.NormalizarEstado(reclamacion.Estado);
        var estadoNuevo = ReclamacionEstados.RespondidaPorPrestadora;

        if (!ReclamacionEstadoService.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new
            {
                mensaje = ReclamacionEstadoService.CrearMensajeTransicionInvalida(estadoAnterior, estadoNuevo),
                estadoActual = estadoAnterior,
                estadoSolicitado = estadoNuevo,
                codigo = "TRANSICION_ESTADO_INVALIDA"
            });
        }

        var respuesta = new RespuestaPrestadora
        {
            ReclamacionId = id,
            PrestadoraId = request.PrestadoraId,
            Respuesta = request.Respuesta,
            DocumentoSoporte = request.DocumentoSoporte
        };

        _db.RespuestasPrestadora.Add(respuesta);
        reclamacion.Estado = estadoNuevo;

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, estadoNuevo, "Respuesta registrada por la prestadora");

        return Ok(respuesta);
    }

    private async Task<string> GenerarNumeroExpediente()
    {
        for (var intento = 0; intento < 10; intento++)
        {
            var numero = $"IND-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(100, 999)}";
            var existe = await _db.Reclamaciones.AnyAsync(x => x.NumeroExpediente == numero);

            if (!existe)
            {
                return numero;
            }
        }

        return $"IND-{DateTime.UtcNow:yyyyMMddHHmmssfffffff}-{Guid.NewGuid():N}";
    }

    private async Task<bool> PuedeVerReclamacion(Reclamacion reclamacion)
    {
        if (User.IsInRole("Administrador") || User.IsInRole("AnalistaDAU") || User.IsInRole("Auditor"))
        {
            return true;
        }

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoId = await ObtenerCiudadanoIdActual();
            return ciudadanoId == reclamacion.CiudadanoId;
        }

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            return prestadoraId == reclamacion.PrestadoraId;
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
