namespace Indotel.Core.DTOs;

public class RespuestaPrestadoraCreateDto
{
    public int PrestadoraId { get; set; }
    public string Respuesta { get; set; } = string.Empty;
    public string DocumentoSoporte { get; set; } = string.Empty;
}
