using System.Security.Claims;
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
[Route("api/resoluciones")]
public class ResolucionesController : ControllerBase
{
    private readonly IndotelDbContext _db;

    public ResolucionesController(IndotelDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? estado,
        [FromQuery] int? tipoResolucionId,
        [FromQuery] int? reclamacionId,
        [FromQuery] int? prestadoraId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 200) pageSize = 200;

        var query = _db.ResolucionesInstitucionales
            .Include(x => x.TipoResolucion)
            .AsQueryable();

        query = await AplicarFiltroVisibilidad(query);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = ResolucionInstitucionalEstados.Normalizar(estado);
            query = query.Where(x => x.Estado == estadoNormalizado);
        }

        if (tipoResolucionId is not null)
        {
            query = query.Where(x => x.TipoResolucionId == tipoResolucionId.Value);
        }

        if (reclamacionId is not null)
        {
            query = query.Where(x => x.ReclamacionId == reclamacionId.Value);
        }

        if (prestadoraId is not null)
        {
            query = query.Where(x => x.PrestadoraId == prestadoraId.Value);
        }

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, data });
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resolucion = await _db.ResolucionesInstitucionales
            .Include(x => x.TipoResolucion)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (resolucion is null) return NotFound(new { mensaje = "Resolucion institucional no encontrada" });
        if (!await PuedeVerResolucion(resolucion)) return Forbid();

        return Ok(resolucion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost]
    public async Task<IActionResult> Create(ResolucionInstitucionalCreateDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
        {
            return BadRequest(new { mensaje = "El titulo es obligatorio" });
        }

        if (string.IsNullOrWhiteSpace(request.Resumen))
        {
            return BadRequest(new { mensaje = "El resumen es obligatorio" });
        }

        var tipoExiste = await _db.TiposResolucion.AnyAsync(x => x.Id == request.TipoResolucionId && x.Activo);
        if (!tipoExiste)
        {
            return BadRequest(new { mensaje = "Tipo de resolucion no valido" });
        }

        if (request.ReclamacionId is not null)
        {
            var reclamacion = await _db.Reclamaciones.FindAsync(request.ReclamacionId.Value);
            if (reclamacion is null) return BadRequest(new { mensaje = "La reclamacion vinculada no existe" });

            if (request.PrestadoraId is null)
            {
                request.PrestadoraId = reclamacion.PrestadoraId;
            }
        }

        if (request.PrestadoraId is not null)
        {
            var prestadoraExiste = await _db.Prestadoras.AnyAsync(x => x.Id == request.PrestadoraId.Value);
            if (!prestadoraExiste) return BadRequest(new { mensaje = "La prestadora vinculada no existe" });
        }

        var numeroResolucion = string.IsNullOrWhiteSpace(request.NumeroResolucion)
            ? await GenerarNumeroResolucion()
            : request.NumeroResolucion.Trim().ToUpperInvariant();

        var numeroExiste = await _db.ResolucionesInstitucionales.AnyAsync(x => x.NumeroResolucion == numeroResolucion);
        if (numeroExiste)
        {
            return Conflict(new { mensaje = "Ya existe una resolucion institucional con este numero" });
        }

        var resolucion = new ResolucionInstitucional
        {
            NumeroResolucion = numeroResolucion,
            Titulo = request.Titulo.Trim(),
            Resumen = request.Resumen.Trim(),
            TipoResolucionId = request.TipoResolucionId,
            Estado = ResolucionInstitucionalEstados.Borrador,
            ReclamacionId = request.ReclamacionId,
            PrestadoraId = request.PrestadoraId,
            UsuarioCreacionId = ObtenerUsuarioIdActual(),
            FechaCreacion = DateTime.UtcNow
        };

        _db.ResolucionesInstitucionales.Add(resolucion);
        await _db.SaveChangesAsync();

        await RegistrarAuditoria(
            resolucion.Id.ToString(),
            "CREAR_RESOLUCION_INSTITUCIONAL",
            "INFO",
            $"Resolucion institucional creada: {resolucion.NumeroResolucion}",
            string.Empty,
            ResolucionInstitucionalEstados.Borrador);

        return CreatedAtAction(nameof(GetById), new { id = resolucion.Id }, resolucion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id)
    {
        var resolucion = await _db.ResolucionesInstitucionales.FindAsync(id);
        if (resolucion is null) return NotFound(new { mensaje = "Resolucion institucional no encontrada" });

        var estadoAnterior = ResolucionInstitucionalEstados.Normalizar(resolucion.Estado);
        var estadoNuevo = ResolucionInstitucionalEstados.Aprobada;

        if (!ResolucionInstitucionalEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Transicion de estado no permitida", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        resolucion.Estado = estadoNuevo;
        resolucion.FechaAprobacion = DateTime.UtcNow;
        resolucion.UsuarioAprobacionId = ObtenerUsuarioIdActual();
        resolucion.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(
            resolucion.Id.ToString(),
            "APROBAR_RESOLUCION_INSTITUCIONAL",
            "INFO",
            $"Resolucion institucional aprobada: {resolucion.NumeroResolucion}",
            estadoAnterior,
            estadoNuevo);

        return Ok(resolucion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/publicar")]
    public async Task<IActionResult> Publicar(int id)
    {
        var resolucion = await _db.ResolucionesInstitucionales.FindAsync(id);
        if (resolucion is null) return NotFound(new { mensaje = "Resolucion institucional no encontrada" });

        var estadoAnterior = ResolucionInstitucionalEstados.Normalizar(resolucion.Estado);
        var estadoNuevo = ResolucionInstitucionalEstados.Publicada;

        if (!ResolucionInstitucionalEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Solo se puede publicar una resolucion aprobada", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        resolucion.Estado = estadoNuevo;
        resolucion.FechaPublicacion = DateTime.UtcNow;
        resolucion.UsuarioPublicacionId = ObtenerUsuarioIdActual();
        resolucion.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(
            resolucion.Id.ToString(),
            "PUBLICAR_RESOLUCION_INSTITUCIONAL",
            "INFO",
            $"Resolucion institucional publicada: {resolucion.NumeroResolucion}",
            estadoAnterior,
            estadoNuevo);

        return Ok(resolucion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPatch("{id:int}/archivar")]
    public async Task<IActionResult> Archivar(int id, ResolucionInstitucionalArchivarDto request)
    {
        var resolucion = await _db.ResolucionesInstitucionales.FindAsync(id);
        if (resolucion is null) return NotFound(new { mensaje = "Resolucion institucional no encontrada" });

        var estadoAnterior = ResolucionInstitucionalEstados.Normalizar(resolucion.Estado);
        var estadoNuevo = ResolucionInstitucionalEstados.Archivada;

        if (!ResolucionInstitucionalEstados.PuedeCambiar(estadoAnterior, estadoNuevo))
        {
            return Conflict(new { mensaje = "Transicion de estado no permitida", estadoActual = estadoAnterior, estadoSolicitado = estadoNuevo });
        }

        resolucion.Estado = estadoNuevo;
        resolucion.MotivoArchivo = request.MotivoArchivo.Trim();
        resolucion.FechaArchivo = DateTime.UtcNow;
        resolucion.UsuarioArchivoId = ObtenerUsuarioIdActual();
        resolucion.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RegistrarAuditoria(
            resolucion.Id.ToString(),
            "ARCHIVAR_RESOLUCION_INSTITUCIONAL",
            "INFO",
            $"Resolucion institucional archivada: {resolucion.NumeroResolucion}",
            estadoAnterior,
            estadoNuevo);

        return Ok(resolucion);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpPost("{id:int}/documento")]
    public async Task<IActionResult> AdjuntarDocumento(int id, ResolucionInstitucionalAdjuntarDocumentoDto request)
    {
        var resolucion = await _db.ResolucionesInstitucionales.FindAsync(id);
        if (resolucion is null) return NotFound(new { mensaje = "Resolucion institucional no encontrada" });

        if (request.DocumentoReclamacionId is null && string.IsNullOrWhiteSpace(request.UrlDocumentoOficial))
        {
            return BadRequest(new { mensaje = "Debe enviar DocumentoReclamacionId o UrlDocumentoOficial" });
        }

        if (request.DocumentoReclamacionId is not null)
        {
            var documento = await _db.DocumentosReclamacion.FindAsync(request.DocumentoReclamacionId.Value);
            if (documento is null) return BadRequest(new { mensaje = "Documento de reclamacion no encontrado" });

            if (resolucion.ReclamacionId is not null && documento.ReclamacionId != resolucion.ReclamacionId.Value)
            {
                return BadRequest(new { mensaje = "El documento no pertenece a la reclamacion vinculada a esta resolucion" });
            }

            resolucion.DocumentoReclamacionId = documento.Id;
        }

        if (!string.IsNullOrWhiteSpace(request.UrlDocumentoOficial))
        {
            resolucion.UrlDocumentoOficial = request.UrlDocumentoOficial.Trim();
        }

        resolucion.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await RegistrarAuditoria(
            resolucion.Id.ToString(),
            "ADJUNTAR_DOCUMENTO_RESOLUCION",
            "INFO",
            $"Documento adjuntado a resolucion institucional {resolucion.NumeroResolucion}",
            resolucion.Estado,
            resolucion.Estado);

        return Ok(resolucion);
    }

    private async Task<IQueryable<ResolucionInstitucional>> AplicarFiltroVisibilidad(IQueryable<ResolucionInstitucional> query)
    {
        if (User.IsInRole("Administrador") || User.IsInRole("AnalistaDAU") || User.IsInRole("Auditor"))
        {
            return query;
        }

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            if (prestadoraId is null) return query.Where(x => false);

            return query.Where(x => x.PrestadoraId == prestadoraId.Value ||
                                    (x.ReclamacionId != null && _db.Reclamaciones
                                        .Where(r => r.Id == x.ReclamacionId.Value)
                                        .Select(r => r.PrestadoraId)
                                        .FirstOrDefault() == prestadoraId.Value));
        }

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoId = await ObtenerCiudadanoIdActual();
            if (ciudadanoId is null) return query.Where(x => false);

            return query.Where(x => x.ReclamacionId != null && _db.Reclamaciones
                .Where(r => r.Id == x.ReclamacionId.Value)
                .Select(r => r.CiudadanoId)
                .FirstOrDefault() == ciudadanoId.Value);
        }

        return query.Where(x => false);
    }

    private async Task<bool> PuedeVerResolucion(ResolucionInstitucional resolucion)
    {
        if (User.IsInRole("Administrador") || User.IsInRole("AnalistaDAU") || User.IsInRole("Auditor"))
        {
            return true;
        }

        if (User.IsInRole("Prestadora"))
        {
            var prestadoraId = await ObtenerPrestadoraIdActual();
            if (prestadoraId is null) return false;
            if (resolucion.PrestadoraId == prestadoraId.Value) return true;

            if (resolucion.ReclamacionId is not null)
            {
                return await _db.Reclamaciones.AnyAsync(x => x.Id == resolucion.ReclamacionId.Value && x.PrestadoraId == prestadoraId.Value);
            }
        }

        if (User.IsInRole("Ciudadano"))
        {
            var ciudadanoId = await ObtenerCiudadanoIdActual();
            if (ciudadanoId is null) return false;

            if (resolucion.ReclamacionId is not null)
            {
                return await _db.Reclamaciones.AnyAsync(x => x.Id == resolucion.ReclamacionId.Value && x.CiudadanoId == ciudadanoId.Value);
            }
        }

        return false;
    }

    private async Task<string> GenerarNumeroResolucion()
    {
        for (var intento = 0; intento < 10; intento++)
        {
            var numero = $"RES-IND-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(100, 999)}";
            var existe = await _db.ResolucionesInstitucionales.AnyAsync(x => x.NumeroResolucion == numero);

            if (!existe)
            {
                return numero;
            }
        }

        return $"RES-IND-{DateTime.UtcNow:yyyyMMddHHmmssfffffff}-{Guid.NewGuid():N}";
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

    private async Task RegistrarAuditoria(
        string entidadId,
        string accion,
        string nivel,
        string detalle,
        string estadoAnterior,
        string estadoNuevo)
    {
        var userId = ObtenerUsuarioIdActual();

        _db.Auditorias.Add(new Auditoria
        {
            UsuarioId = userId == 0 ? null : userId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = "ResolucionInstitucional",
            EntidadId = entidadId,
            Accion = accion,
            Nivel = string.IsNullOrWhiteSpace(nivel) ? "INFO" : nivel.ToUpperInvariant(),
            Detalle = detalle,
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
