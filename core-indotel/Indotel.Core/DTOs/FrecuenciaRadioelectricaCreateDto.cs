namespace Indotel.Core.DTOs;

public class FrecuenciaRadioelectricaCreateDto
{
    public decimal RangoInicioMHz { get; set; }
    public decimal RangoFinMHz { get; set; }
    public string Banda { get; set; } = string.Empty;
    public string ServicioUso { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}
