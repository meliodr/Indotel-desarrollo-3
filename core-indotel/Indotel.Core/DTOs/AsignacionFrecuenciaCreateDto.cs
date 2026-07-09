namespace Indotel.Core.DTOs;

public class AsignacionFrecuenciaCreateDto
{
    public int FrecuenciaRadioelectricaId { get; set; }
    public int? PrestadoraId { get; set; }
    public string EntidadAsignada { get; set; } = string.Empty;
    public string UsoAutorizado { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime? FechaFin { get; set; }
    public string Observacion { get; set; } = string.Empty;
}
