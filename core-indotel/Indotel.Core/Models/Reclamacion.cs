namespace Indotel.Core.Models;

public class Reclamacion
{
    public int Id { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public int CiudadanoId { get; set; }
    public int PrestadoraId { get; set; }
    public int ServicioTelecomId { get; set; }
    public int? TipoReclamacionId { get; set; }
    public int? MotivoReclamacionId { get; set; }
    public string CanalRecepcion { get; set; } = "WEB";
    public string Prioridad { get; set; } = "MEDIA";
    public string Provincia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "RECIBIDA";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public TipoReclamacion? TipoReclamacion { get; set; }
    public MotivoReclamacion? MotivoReclamacion { get; set; }
}
