namespace Indotel.Core.DTOs;

public class CerrarReclamacionDto
{
    public string MotivoCierre { get; set; } = string.Empty;
    public string ComentarioCierre { get; set; } = string.Empty;
    public bool? ConformidadCiudadano { get; set; }
}
