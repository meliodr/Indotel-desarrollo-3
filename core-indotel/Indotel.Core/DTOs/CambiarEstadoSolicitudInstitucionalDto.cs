namespace Indotel.Core.DTOs;

public class CambiarEstadoSolicitudInstitucionalDto
{
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public DateTime? FechaVencimiento { get; set; }
    public int? ResolucionInstitucionalId { get; set; }
}
