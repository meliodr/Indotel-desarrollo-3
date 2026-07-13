using System.Security.Claims;
using Indotel.Core.Data;
using Indotel.Core.Helpers;
using Indotel.Core.Models;
using Indotel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class DocumentosController : ControllerBase
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> ExtensionesPermitidas = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private readonly IndotelDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly CurrentUserService _currentUser;

    public DocumentosController(
        IndotelDbContext db,
        IWebHostEnvironment environment,
        CurrentUserService currentUser)
    {
        _db = db;
        _environment = environment;
        _currentUser = currentUser;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("reclamaciones/{reclamacionId:int}/documentos")]
    public async Task<IActionResult> GetByReclamacion(
        int reclamacionId,
        CancellationToken cancellationToken)
    {
        var reclamacion = await _db.Reclamaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reclamacionId, cancellationToken);

        if (reclamacion is null)
        {
            return Problema(
                StatusCodes.Status404NotFound,
                "RECLAMACION_NO_ENCONTRADA",
                "Reclamacion no encontrada");
        }

        if (!await _currentUser.PuedeVerReclamacionAsync(reclamacion, cancellationToken))
        {
            return Problema(
                StatusCodes.Status403Forbidden,
                "RECLAMACION_SIN_ACCESO",
                "No tiene permiso para consultar los documentos de esta reclamacion");
        }

        var documentos = await _db.DocumentosReclamacion
            .AsNoTracking()
            .Where(x => x.ReclamacionId == reclamacionId)
            .OrderByDescending(x => x.FechaSubida)
            .Select(x => new
            {
                x.Id,
                x.ReclamacionId,
                x.NombreArchivo,
                x.TipoContenido,
                x.FechaSubida
            })
            .ToListAsync(cancellationToken);

        return Ok(documentos);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("documentos/{id:int}/descargar")]
    public async Task<IActionResult> Descargar(int id, CancellationToken cancellationToken)
    {
        var documento = await _db.DocumentosReclamacion
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (documento is null)
        {
            return Problema(
                StatusCodes.Status404NotFound,
                "DOCUMENTO_NO_ENCONTRADO",
                "Documento no encontrado");
        }

        var reclamacion = await _db.Reclamaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documento.ReclamacionId, cancellationToken);

        if (reclamacion is null)
        {
            return Problema(
                StatusCodes.Status404NotFound,
                "RECLAMACION_NO_ENCONTRADA",
                "Reclamacion no encontrada");
        }

        if (!await _currentUser.PuedeVerReclamacionAsync(reclamacion, cancellationToken))
        {
            return Problema(
                StatusCodes.Status403Forbidden,
                "DOCUMENTO_SIN_ACCESO",
                "No tiene permiso para descargar este documento");
        }

        var rutaFisica = ObtenerRutaFisicaSegura(documento.RutaArchivo);
        if (rutaFisica is null)
        {
            await RegistrarAuditoriaAsync(
                "Documento",
                id.ToString(),
                "DESCARGA_DOCUMENTO_BLOQUEADA",
                "WARN",
                "Ruta de documento invalida",
                cancellationToken);

            return Problema(
                StatusCodes.Status400BadRequest,
                "RUTA_DOCUMENTO_INVALIDA",
                "Ruta de documento invalida");
        }

        if (!System.IO.File.Exists(rutaFisica))
        {
            await RegistrarAuditoriaAsync(
                "Documento",
                id.ToString(),
                "DESCARGA_DOCUMENTO_NO_ENCONTRADO",
                "WARN",
                "Archivo fisico no encontrado",
                cancellationToken);

            return Problema(
                StatusCodes.Status404NotFound,
                "ARCHIVO_FISICO_NO_ENCONTRADO",
                "Archivo fisico no encontrado");
        }

        await RegistrarAuditoriaAsync(
            "Documento",
            id.ToString(),
            "DESCARGA_DOCUMENTO",
            "INFO",
            $"Documento descargado: {documento.NombreArchivo}",
            cancellationToken);

        var tipoContenido = string.IsNullOrWhiteSpace(documento.TipoContenido)
            ? "application/octet-stream"
            : documento.TipoContenido;
        var nombreDescarga = string.IsNullOrWhiteSpace(documento.NombreArchivo)
            ? $"documento-{documento.Id}{Path.GetExtension(rutaFisica)}"
            : Path.GetFileName(documento.NombreArchivo);

        return PhysicalFile(rutaFisica, tipoContenido, nombreDescarga, enableRangeProcessing: true);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Ciudadano")]
    [RequestSizeLimit(MaxFileSizeBytes + 1024 * 1024)]
    [HttpPost("reclamaciones/{reclamacionId:int}/documentos")]
    public async Task<IActionResult> Upload(
        int reclamacionId,
        IFormFile? archivo,
        CancellationToken cancellationToken)
    {
        var reclamacion = await _db.Reclamaciones
            .FirstOrDefaultAsync(x => x.Id == reclamacionId, cancellationToken);

        if (reclamacion is null)
        {
            return Problema(
                StatusCodes.Status404NotFound,
                "RECLAMACION_NO_ENCONTRADA",
                "Reclamacion no encontrada");
        }

        if (!await _currentUser.PuedeVerReclamacionAsync(reclamacion, cancellationToken))
        {
            return Problema(
                StatusCodes.Status403Forbidden,
                "RECLAMACION_SIN_ACCESO",
                "No tiene permiso para agregar documentos a esta reclamacion");
        }

        if (ReclamacionEstadoService.EsFinal(reclamacion.Estado))
        {
            return Problema(
                StatusCodes.Status409Conflict,
                "RECLAMACION_FINALIZADA",
                "No se pueden subir documentos a una reclamacion cerrada, rechazada o archivada");
        }

        if (archivo is null || archivo.Length == 0)
        {
            return Problema(
                StatusCodes.Status400BadRequest,
                "ARCHIVO_OBLIGATORIO",
                "Debe enviar un archivo");
        }

        if (archivo.Length > MaxFileSizeBytes)
        {
            return Problema(
                StatusCodes.Status400BadRequest,
                "ARCHIVO_DEMASIADO_GRANDE",
                "El archivo no puede superar 5MB");
        }

        var nombreOriginal = Path.GetFileName(archivo.FileName ?? string.Empty);
        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(nombreOriginal) || !ExtensionesPermitidas.Contains(extension))
        {
            return Problema(
                StatusCodes.Status400BadRequest,
                "TIPO_ARCHIVO_NO_PERMITIDO",
                "Tipo de archivo no permitido. Solo PDF, JPG, JPEG o PNG");
        }

        await using var input = archivo.OpenReadStream();
        var (contenidoValido, tipoContenidoReal) = await FileSignatureValidator.ValidarAsync(
            input,
            extension,
            cancellationToken);

        if (!contenidoValido)
        {
            return Problema(
                StatusCodes.Status400BadRequest,
                "CONTENIDO_ARCHIVO_INVALIDO",
                "El contenido del archivo no corresponde con su extension");
        }

        var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
        var carpetaRelativa = Path.Combine("uploads", "reclamaciones", reclamacionId.ToString());
        var carpetaFisica = Path.Combine(_environment.ContentRootPath, carpetaRelativa);
        Directory.CreateDirectory(carpetaFisica);

        var rutaFisica = Path.Combine(carpetaFisica, nombreSeguro);

        try
        {
            await using (var output = new FileStream(
                             rutaFisica,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             81920,
                             useAsync: true))
            {
                await input.CopyToAsync(output, cancellationToken);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var documento = new DocumentoReclamacion
            {
                ReclamacionId = reclamacionId,
                NombreArchivo = nombreOriginal,
                TipoContenido = tipoContenidoReal,
                RutaArchivo = Path.Combine(carpetaRelativa, nombreSeguro).Replace("\\", "/"),
                FechaSubida = DateTime.UtcNow
            };

            _db.DocumentosReclamacion.Add(documento);
            _db.Auditorias.Add(CrearAuditoria(
                "Documento",
                "PENDIENTE",
                "SUBIR_DOCUMENTO",
                "INFO",
                $"Documento subido a reclamacion {reclamacionId}"));

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetByReclamacion), new { reclamacionId }, new
            {
                documento.Id,
                documento.ReclamacionId,
                documento.NombreArchivo,
                documento.TipoContenido,
                documento.FechaSubida
            });
        }
        catch
        {
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            throw;
        }
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpDelete("documentos/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var documento = await _db.DocumentosReclamacion
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (documento is null)
        {
            return Problema(
                StatusCodes.Status404NotFound,
                "DOCUMENTO_NO_ENCONTRADO",
                "Documento no encontrado");
        }

        var rutaFisica = ObtenerRutaFisicaSegura(documento.RutaArchivo);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.DocumentosReclamacion.Remove(documento);
        _db.Auditorias.Add(CrearAuditoria(
            "Documento",
            id.ToString(),
            "ELIMINAR_DOCUMENTO",
            "WARN",
            "Documento eliminado"));
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (rutaFisica is not null && System.IO.File.Exists(rutaFisica))
        {
            System.IO.File.Delete(rutaFisica);
        }

        return Ok(new
        {
            mensaje = "Documento eliminado correctamente",
            correlationId = HttpContext.TraceIdentifier
        });
    }

    private ObjectResult Problema(int status, string codigo, string mensaje)
    {
        return ApiProblemFactory.Create(HttpContext, status, codigo, mensaje);
    }

    private string? ObtenerRutaFisicaSegura(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;
        if (Path.IsPathRooted(rutaRelativa)) return null;
        if (rutaRelativa.Contains("..", StringComparison.Ordinal)) return null;

        var contentRoot = Path.GetFullPath(_environment.ContentRootPath);
        var uploadsRoot = Path.GetFullPath(Path.Combine(contentRoot, "uploads"));
        var rutaFisica = Path.GetFullPath(Path.Combine(
            contentRoot,
            rutaRelativa.Replace("/", Path.DirectorySeparatorChar.ToString())));

        return rutaFisica.StartsWith(
            uploadsRoot + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)
            ? rutaFisica
            : null;
    }

    private async Task RegistrarAuditoriaAsync(
        string entidad,
        string entidadId,
        string accion,
        string nivel,
        string detalle,
        CancellationToken cancellationToken)
    {
        _db.Auditorias.Add(CrearAuditoria(entidad, entidadId, accion, nivel, detalle));
        await _db.SaveChangesAsync(cancellationToken);
    }

    private Auditoria CrearAuditoria(
        string entidad,
        string entidadId,
        string accion,
        string nivel,
        string detalle)
    {
        return new Auditoria
        {
            UsuarioId = _currentUser.UsuarioId,
            UsuarioCorreo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            UsuarioRol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            Nivel = string.IsNullOrWhiteSpace(nivel) ? "INFO" : nivel.ToUpperInvariant(),
            Detalle = detalle ?? string.Empty,
            EstadoAnterior = string.Empty,
            EstadoNuevo = string.Empty,
            MetodoHttp = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier,
            Fecha = DateTime.UtcNow
        };
    }
}
