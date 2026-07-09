using System.Security.Claims;
using Indotel.Core.Constants;
using Indotel.Core.Data;
using Indotel.Core.DTOs;
using Indotel.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize(Roles = "Administrador,AnalistaDAU,Auditor")]
[ApiController]
[Route("api/licencias-tecnicas")]
public class LicenciasTecnicasController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public LicenciasTecnicasController(IndotelDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] bool soloPorVencer = false)
    {
        var query = _db.LicenciasTecnicas
            .Include(x => x.FrecuenciaRadioelectrica)
            .Include(x => x.Prestadora)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(x => x.Estado == LicenciaTecnicaEstados.Normalizar(estado));
        }

        if (soloPorVencer)
        {
            var limite = DateTime.UtcNow.AddDays(60);
            query = query.Where(x => x.FechaVencimiento <= limite && x.Estado != LicenciaTecnicaEstados.Cancelada);
        }

        var data = await query.OrderBy(x => x.FechaVencimiento).ToListAsync();
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var licencia = await _db.LicenciasTecnicas
            .Include(x => x.FrecuenciaRadioelectrica)
            .Include(x => x.Prestadora)
            .FirstOrDefaultAsync(x => x.Id == id);

        return licencia is null ? NotFound(new { mensaje = "Licencia tecnica no encontrada" }) : Ok(licencia);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(LicenciaTecnicaCreateDto request)
    {
        var frecuencia = await _db.FrecuenciasRadioelectricas.FindAsync(request.FrecuenciaRadioelectricaId);
        if (frecuencia is null) return BadRequest(new { mensaje = "Frecuencia no encontrada" });

        if (request.PrestadoraId is not null)
        {
            var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId.Value);
            if (!prestadoraExiste) return BadRequest(new { mensaje = "Prestadora no encontrada" });
        }

        if (request.ResolucionInstitucionalId is not null)
        {
            var resolucionExiste = await _db.ResolucionesInstitucionales.AnyAsync(x => x.Id == request.ResolucionInstitucionalId.Value);
            if (!resolucionExiste) return BadRequest(new { mensaje = "Resolucion institucional no encontrada" });
        }

        var numero = string.IsNullOrWhiteSpace(request.NumeroLicencia)
            ? await GenerarNumeroLicencia()
            : request.NumeroLicencia.Trim().ToUpperInvariant();

        var existeNumero = await _db.LicenciasTecnicas.AnyAsync(x => x.NumeroLicencia == numero);
        if (existeNumero)
        {
            return Conflict(new { mensaje = "Ya existe una licencia tecnica con este numero" });
        }

        var licencia = new LicenciaTecnica
        {
            NumeroLicencia = numero,
            PrestadoraId = request.PrestadoraId,
            EntidadAsignada = request.EntidadAsignada.Trim(),
            FrecuenciaRadioelectricaId = frecuencia.Id,
            Estado = LicenciaTecnicaEstados.Solicitada,
            FechaInicio = request.FechaInicio ?? DateTime.UtcNow,
            FechaVencimiento = request.FechaVencimiento ?? DateTime.UtcNow.AddYears(1),
            ResolucionInstitucionalId = request.ResolucionInstitucionalId,
            UsuarioCreacionId = ObtenerUsuarioIdActual(),
            FechaCreacion = DateTime.UtcNow
        };

        _db.LicenciasTecnicas.Add(licencia);
        await _db.SaveChangesAsync();

        await RegistrarAuditoria(licencia.Id.ToString(), "CREAR_LICENCIA_TECNICA", "INFO", $"Licencia tecnica creada: {licencia.NumeroLicencia}", string.Empty, licencia.Estado);

        return CreatedAtAction(nameof(GetById), new { id = licencia.Id }, licencia);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoLicenciaTecnicaDto request)
    {
        var licencia = await _db.LicenciasTecnicas.FindAsync(id);
        if (licencia is null) return NotFound(new { mensaje = "Licencia tecnica no encontrada" });

        var estadoAnterior = LicenciaTecnicaEstados.Normalizar(licencia.Estado);
        var estadoNuevo = LicenciaTecnicaEstados.Normalizar(request.EstadoNuevo);

        if (!LicenciaTecnicaEstados.Existe(estadoNuevo))
        {
            return BadRequest(new { mensaje = "Estado no permitido" });
        }

        if (!LicenciaTecnicaEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Transicion de estado no permitida", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        licencia.Estado = estadoNuevo;
        licencia.FechaActualizacion = DateTime.UtcNow;

        if (estadoNuevo == LicenciaTecnicaEstados.Activa)
        {
            var frecuencia = await _db.FrecuenciasRadioelectricas.FindAsync(licencia.FrecuenciaRadioelectricaId);
            if (frecuencia is not null)
            {
                frecuencia.Estado = FrecuenciaEstados.Asignada;
                frecuencia.FechaActualizacion = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(licencia.Id.ToString(), "CAMBIO_ESTADO_LICENCIA_TECNICA", "INFO", request.Comentario.Trim(), estadoAnterior, estadoNuevo);

        return Ok(licencia);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, CancelarLicenciaTecnicaDto request)
    {
        var licencia = await _db.LicenciasTecnicas.FindAsync(id);
        if (licencia is null) return NotFound(new { mensaje = "Licencia tecnica no encontrada" });

        var estadoAnterior = LicenciaTecnicaEstados.Normalizar(licencia.Estado);
        if (estadoAnterior == LicenciaTecnicaEstados.Cancelada)
        {
            return Conflict(new { mensaje = "La licencia ya esta cancelada" });
        }

        licencia.Estado = LicenciaTecnicaEstados.Cancelada;
        licencia.FechaCancelacion = DateTime.UtcNow;
        licencia.MotivoCancelacion = request.MotivoCancelacion.Trim();
        licencia.UsuarioCancelacionId = ObtenerUsuarioIdActual();
        licencia.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(licencia.Id.ToString(), "CANCELAR_LICENCIA_TECNICA", "WARN", request.MotivoCancelacion.Trim(), estadoAnterior, LicenciaTecnicaEstados.Cancelada);

        return Ok(licencia);
    }

    private async Task<string> GenerarNumeroLicencia()
    {
        for (var intento = 0; intento < 10; intento++)
        {
            var numero = $"LIC-IND-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(100, 999)}";
            var existe = await _db.LicenciasTecnicas.AnyAsync(x => x.NumeroLicencia == numero);
            if (!existe) return numero;
        }

        return $"LIC-IND-{DateTime.UtcNow:yyyyMMddHHmmssfffffff}-{Guid.NewGuid():N}";
    }

    private int ObtenerUsuarioIdActual()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);
        return userId;
    }

    private async Task RegistrarAuditoria(string entidadId, string accion, string nivel, string detalle, string estadoAnterior, string estadoNuevo)
    {
        var userId = ObtenerUsuarioIdActual();
        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = userId == 0 ? null : userId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = "LicenciaTecnica",
            EntidadId = entidadId,
            Accion = accion,
            Nivel = string.IsNullOrWhiteSpace(nivel) ? "INFO" : nivel.ToUpperInvariant(),
            Detalle = detalle ?? string.Empty,
            EstadoAnterior = estadoAnterior ?? string.Empty,
            EstadoNuevo = estadoNuevo ?? string.Empty,
            MetodoHttp = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier,
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
