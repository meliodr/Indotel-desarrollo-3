using Indotel.Core.Constants;

namespace Indotel.Core.Models;

public class ResolucionInstitucional
{
    public int Id { get; set; }
    public string NumeroResolucion { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public int TipoResolucionId { get; set; }
    public string Estado { get; set; } = ResolucionInstitucionalEstados.Borrador;
    public int? ReclamacionId { get; set; }
    public int? PrestadoraId { get; set; }
    public int? DocumentoReclamacionId { get; set; }
    public string UrlDocumentoOficial { get; set; } = string.Empty;
    public DateTime? FechaAprobacion { get; set; }
    public DateTime? FechaPublicacion { get; set; }
    public DateTime? FechaArchivo { get; set; }
    public string MotivoArchivo { get; set; } = string.Empty;
    public int UsuarioCreacionId { get; set; }
    public int? UsuarioAprobacionId { get; set; }
    public int? UsuarioPublicacionId { get; set; }
    public int? UsuarioArchivoId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public TipoResolucion? TipoResolucion { get; set; }
    public Reclamacion? Reclamacion { get; set; }
    public Prestadora? Prestadora { get; set; }
    public DocumentoReclamacion? DocumentoReclamacion { get; set; }
}
