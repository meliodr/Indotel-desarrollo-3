namespace Indotel.Core.DTOs;

public class RenovarSolicitudInstitucionalDto
{
    public DateTime? NuevaFechaVencimiento { get; set; }
    public string Comentario { get; set; } = string.Empty;
}
