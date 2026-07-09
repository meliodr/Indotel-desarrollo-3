using System.Security.Claims;
using Indotel.Core.Data;
using Indotel.Core.Models;
using Indotel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class DocumentosController : ControllerBase
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly string[] ExtensionesPermitidas =
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private readonly IndotelDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public DocumentosController(IndotelDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Auditor,Prestadora,Ciudadano")]
    [HttpGet("reclamaciones/{reclamacionId:int}/documentos")]
    public async Task<IActionResult> GetByReclamacion(int reclamacionId)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(reclamacionId);
        if (reclamacion is null) return NotFound(new { mensaje = "Reclamacion no encontrada" });

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        var documentos = await _db.DocumentosReclamacion
            .Where(x => x.ReclamacionId == reclamacionId)
            .OrderByDescending(x => x.FechaSubida)
            .ToListAsync();

        return Ok(documentos);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU,Ciudadano")]
    [HttpPost("reclamaciones/{reclamacionId:int}/documentos")]
    public async Task<IActionResult> Upload(int reclamacionId, IFormFile? archivo)
    {
        var reclamacion = await _db.Reclamaciones.FindAsync(reclamacionId);
        if (reclamacion is null) return NotFound(new { mensaje = "Reclamacion no encontrada" });

        if (!await PuedeVerReclamacion(reclamacion)) return Forbid();

        if (ReclamacionEstadoService.EsFinal(reclamacion.Estado))
        {
            return Conflict(new { mensaje = "No se pueden subir documentos a una reclamacion cerrada, rechazada o archivada" });
        }

        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe enviar un archivo" });
        }

        if (archivo.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { mensaje = "El archivo no puede superar 5MB" });
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            return BadRequest(new { mensaje = "Tipo de archivo no permitido. Solo PDF, JPG, JPEG o PNG" });
        }

        var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
        var carpetaRelativa = Path.Combine("uploads", "reclamaciones", reclamacionId.ToString());
        var carpetaFisica = Path.Combine(_environment.ContentRootPath, carpetaRelativa);

        Directory.CreateDirectory(carpetaFisica);

        var rutaFisica = Path.Combine(carpetaFisica, nombreSeguro);
        await using (var stream = System.IO.File.Create(rutaFisica))
        {
            await archivo.CopyToAsync(stream);
        }

        var documento = new DocumentoReclamacion
        {
            ReclamacionId = reclamacionId,
            NombreArchivo = Path.GetFileName(archivo.FileName),
            TipoContenido = archivo.ContentType,
            RutaArchivo = Path.Combine(carpetaRelativa, nombreSeguro).Replace("\\", "/"),
            FechaSubida = DateTime.UtcNow
        };

        _db.DocumentosReclamacion.Add(documento);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByReclamacion), new { reclamacionId }, documento);
    }

    [Authorize(Roles = "Administrador,AnalistaDAU")]
    [HttpDelete("documentos/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var documento = await _db.DocumentosReclamacion.FindAsync(id);
        if (documento is null) return NotFound(new { mensaje = "Documento no encontrado" });

        var rutaFisica = Path.Combine(_environment.ContentRootPath, documento.RutaArchivo.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (System.IO.File.Exists(rutaFisica))
        {
            System.IO.File.Delete(rutaFisica);
        }

        _db.DocumentosReclamacion.Remove(documento);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Documento eliminado correctamente" });
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
}
