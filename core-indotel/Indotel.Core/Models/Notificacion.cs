namespace Indotel.Core.Models;

public class Notificacion
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public int? CiudadanoId { get; set; }
    public int? PrestadoraId { get; set; }
    public int? ReclamacionId { get; set; }
    public string Canal { get; set; } = "INTERNA";
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLectura { get; set; }
    public DateTime? FechaEnvio { get; set; }
}
