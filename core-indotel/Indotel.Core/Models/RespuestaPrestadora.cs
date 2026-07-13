namespace Indotel.Core.Models;

public class RespuestaPrestadora
{
    public int Id { get; set; }
    public int ReclamacionId { get; set; }
    public int PrestadoraId { get; set; }
    public string Respuesta { get; set; } = string.Empty;
    public string DocumentoSoporte { get; set; } = string.Empty;
    public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;
}
