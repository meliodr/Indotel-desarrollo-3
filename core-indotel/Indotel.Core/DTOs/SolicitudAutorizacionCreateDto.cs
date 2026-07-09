namespace Indotel.Core.DTOs;

public class SolicitudAutorizacionCreateDto
{
    public string SolicitanteNombre { get; set; } = string.Empty;
    public string SolicitanteRnc { get; set; } = string.Empty;
    public int? PrestadoraId { get; set; }
    public int TipoAutorizacionId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
