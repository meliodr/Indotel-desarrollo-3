namespace Indotel.Core.DTOs;

public class NotificacionCreateDto
{
    public int? UsuarioId { get; set; }
    public int? CiudadanoId { get; set; }
    public int? PrestadoraId { get; set; }
    public int? ReclamacionId { get; set; }
    public string Canal { get; set; } = "INTERNA";
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
