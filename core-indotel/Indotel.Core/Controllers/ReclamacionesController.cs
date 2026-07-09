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
        var query = _db.Reclamaciones
            .Include(x => x.TipoReclamacion)
            .Include(x => x.MotivoReclamacion)
            .AsQueryable();

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

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor")]
    [HttpGet("sla/vencidas")]
    public async Task<IActionResult> GetVencidas()
    {
        var ahora = DateTime.UtcNow;

        var data = await _db.Reclamaciones
            .Include(x => x.TipoReclamacion)
            .Include(x => x.MotivoReclamacion)
            .Where(x => x.EstaVencida ||
                        (x.FechaLimiteRespuesta != null &&
                         x.FechaRespuestaPrestadora == null &&
                         ahora > x.FechaLimiteRespuesta.Value))
            .OrderBy(x => x.FechaLimiteRespuesta)
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("sla/marcar-vencidas")]
    public async Task<IActionResult> MarcarVencidas()
    {
        var ahora = DateTime.UtcNow;
        var vencidas = await _db.Reclamaciones
            .Where(x => x.Estado == ReclamacionEstados.EnviadaAPrestadora &&
                        x.FechaLimiteRespuesta != null &&
                        x.FechaRespuestaPrestadora == null &&
                        ahora > x.FechaLimiteRespuesta.Value &&
                        !x.EstaVencida)
            .ToListAsync();

        var userId = ObtenerUsuarioIdActual();

        foreach (var reclamacion in vencidas)
        {
            var estadoAnterior = reclamacion.Estado;
            reclamacion.Estado = ReclamacionEstados.Vencida;
            reclamacion.EstaVencida = true;
            reclamacion.FechaMarcadaVencida = ahora;

            _db.HistorialReclamaciones.Add(new HistorialReclamacion
            {
                ReclamacionId = reclamacion.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = ReclamacionEstados.Vencida,
                Comentario = "Reclamacion marcada como vencida por SLA",
                UsuarioId = userId,
                FechaCambio = ahora
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { cantidad = vencidas.Count });
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reclamacion = await _db.Reclamaciones
            .Include(x => x.TipoReclamacion)
            .Include(x => x.MotivoReclamacion)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (reclamacion is null) return NotFound();

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("expediente/{numero}")]
    public async Task<IActionResult> GetByNumeroExpediente(string numero)
    {
        var reclamacion = await _db.Reclamaciones
            .Include(x => x.TipoReclamacion)
            .Include(x => x.MotivoReclamacion)
            .FirstOrDefaultAsync(x => x.NumeroExpediente == numero);

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

        if (!ReclamacionClasificacion.EsCanalValido(request.CanalRecepcion))
        {
            return BadRequest(new { mensaje = "Canal de recepcion no permitido" });
        }

        if (!ReclamacionClasificacion.EsPrioridadValida(request.Prioridad))
        {
            return BadRequest(new { mensaje = "Prioridad no permitida" });
        }

        if (request.TipoReclamacionId is not null)
        {
            var tipoExiste = await _db.TiposReclamacion.AnyAsync(x => x.Id == request.TipoReclamacionId.Value && x.Activo);
            if (!tipoExiste)
            {
                return BadRequest(new { mensaje = "Tipo de reclamacion no valido" });
            }
        }

        if (request.MotivoReclamacionId is not null)
        {
            var motivo = await _db.MotivosReclamacion.FirstOrDefaultAsync(x => x.Id == request.MotivoReclamacionId.Value && x.Activo);
            if (motivo is null)
            {
                return BadRequest(new { mensaje = "Motivo de reclamacion no valido" });
            }

            if (request.TipoReclamacionId is not null && motivo.TipoReclamacionId != request.TipoReclamacionId.Value)
            {
                return BadRequest(new { mensaje = "El motivo no corresponde al tipo de reclamacion" });
            }
        }

        var reclamacion = new Reclamacion
        {
            NumeroExpediente = await GenerarNumeroExpediente(),
            CiudadanoId = ciudadanoId,
            PrestadoraId = request.PrestadoraId,
            ServicioTelecomId = request.ServicioTelecomId,
            TipoReclamacionId = request.TipoReclamacionId,
            MotivoReclamacionId = request.MotivoReclamacionId,
            CanalRecepcion = ReclamacionClasificacion.Normalizar(request.CanalRecepcion),
            Prioridad = ReclamacionClasificacion.Normalizar(request.Prioridad),
            Provincia = request.Provincia.Trim(),
            Municipio = request.Municipio.Trim(),
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
        AplicarSlaPorCambioEstado(reclamacion, estadoNuevo);

        if (estadoNuevo == ReclamacionEstados.Resuelta)
        {
            reclamacion.FechaResolucion ??= DateTime.UtcNow;
            reclamacion.UsuarioResolucionId ??= ObtenerUsuarioIdActual();
        }

        if (estadoNuevo == ReclamacionEstados.Cerrada)
        {
            reclamacion.FechaCierre = DateTime.UtcNow;
            reclamacion.UsuarioCierreId ??= ObtenerUsuarioIdActual();
        }

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, estadoNuevo, request.Comentario);

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("{id:int}/resolver")]
    public async Task<IActionResult> Resolver(int id, ResolverReclamacionDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.ResultadoResolucion))
        {
            return BadRequest(new { mensaje = "El resultado de resolucion es obligatorio" });
        }

        if (string.IsNullOrWhiteSpace(request.ComentarioResolucion))
        {
            return BadRequest(new { mensaje = "El comentario de resolucion es obligatorio" });
        }

        var estadoAnterior = ReclamacionEstadoService.NormalizarEstado(reclamacion.Estado);
        var estadoNuevo = ReclamacionEstados.Resuelta;

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
        reclamacion.FechaResolucion = DateTime.UtcNow;
        reclamacion.ResultadoResolucion = request.ResultadoResolucion.Trim();
        reclamacion.ComentarioResolucion = request.ComentarioResolucion.Trim();
        reclamacion.FundamentoResolucion = request.FundamentoResolucion.Trim();
        reclamacion.AccionOrdenada = request.AccionOrdenada.Trim();
        reclamacion.MontoAjuste = request.MontoAjuste;
        reclamacion.UsuarioResolucionId = ObtenerUsuarioIdActual();

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, estadoNuevo, request.ComentarioResolucion.Trim());

        return Ok(reclamacion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("{id:int}/cerrar")]
    public async Task<IActionResult> Cerrar(int id, CerrarReclamacionDto request)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(id);
        if (reclamacion is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.MotivoCierre))
        {
            return BadRequest(new { mensaje = "El motivo de cierre es obligatorio" });
        }

        var estadoAnterior = ReclamacionEstadoService.NormalizarEstado(reclamacion.Estado);
        var estadoNuevo = ReclamacionEstados.Cerrada;

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
        reclamacion.FechaCierre = DateTime.UtcNow;
        reclamacion.MotivoCierre = request.MotivoCierre.Trim();
        reclamacion.ComentarioCierre = request.ComentarioCierre.Trim();
        reclamacion.ConformidadCiudadano = request.ConformidadCiudadano;
        reclamacion.UsuarioCierreId = ObtenerUsuarioIdActual();

        await _db.SaveChangesAsync();
        await RegistrarHistorial(id, estadoAnterior, estadoNuevo, request.MotivoCierre.Trim());

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
        reclamacion.FechaRespuestaPrestadora = DateTime.UtcNow;
        reclamacion.EstaVencida = false;

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

    private static void AplicarSlaPorCambioEstado(Reclamacion reclamacion, string estadoNuevo)
    {
        var ahora = DateTime.UtcNow;

        if (estadoNuevo == ReclamacionEstados.EnviadaAPrestadora)
        {
            reclamacion.FechaEnvioPrestadora ??= ahora;
            reclamacion.DiasHabilesSla ??= ReclamacionSla.DiasHabilesRespuestaPrestadora;
            reclamacion.FechaLimiteRespuesta ??= ReclamacionSlaService.SumarDiasHabiles(ahora, reclamacion.DiasHabilesSla.Value);
            reclamacion.EstaVencida = false;
            reclamacion.FechaMarcadaVencida = null;
        }

        if (estadoNuevo == ReclamacionEstados.RespondidaPorPrestadora)
        {
            reclamacion.FechaRespuestaPrestadora ??= ahora;
            reclamacion.EstaVencida = false;
        }

        if (estadoNuevo == ReclamacionEstados.Vencida)
        {
            reclamacion.EstaVencida = true;
            reclamacion.FechaMarcadaVencida ??= ahora;
        }
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

    private int ObtenerUsuarioIdActual()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdText, out var userId);
        return userId;
    }

    private async Task RegistrarHistorial(int reclamacionId, string estadoAnterior, string estadoNuevo, string comentario)
    {
        _db.HistorialReclamaciones.Add(new HistorialReclamacion
        {
            ReclamacionId = reclamacionId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            Comentario = comentario,
            UsuarioId = ObtenerUsuarioIdActual(),
            FechaCambio = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
