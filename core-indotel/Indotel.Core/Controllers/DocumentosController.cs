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
        var reclamacionExiste = await _db.Reclamaciones.AnyAsync(x => x.Id == reclamacionId);
        if (!reclamacionExiste) return NotFound(new { mensaje = "Reclamacion no encontrada" });

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
}
