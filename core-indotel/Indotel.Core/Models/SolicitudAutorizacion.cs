using Indotel.Core.Constants;

namespace Indotel.Core.Models;

public class SolicitudAutorizacion
{
    public int Id { get; set; }
    public string NumeroSolicitud { get; set; } = string.Empty;
    public string SolicitanteNombre { get; set; } = string.Empty;
    public string SolicitanteRnc { get; set; } = string.Empty;
    public int? PrestadoraId { get; set; }
    public int TipoAutorizacionId { get; set; }
    public string Estado { get; set; } = SolicitudInstitucionalEstados.Recibida;
    public string Descripcion { get; set; } = string.Empty;
    public string ComentarioRevision { get; set; } = string.Empty;
    public string MotivoRechazo { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRevision { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaRenovacion { get; set; }
    public int? UsuarioResponsableId { get; set; }
    public int? ResolucionInstitucionalId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public Prestadora? Prestadora { get; set; }
    public TipoAutorizacion? TipoAutorizacion { get; set; }
    public ResolucionInstitucional? ResolucionInstitucional { get; set; }
}
