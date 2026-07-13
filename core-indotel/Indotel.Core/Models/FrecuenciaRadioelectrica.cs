using Indotel.Core.Constants;

namespace Indotel.Core.Models;

public class FrecuenciaRadioelectrica
{
    public int Id { get; set; }
    public decimal RangoInicioMHz { get; set; }
    public decimal RangoFinMHz { get; set; }
    public string Banda { get; set; } = string.Empty;
    public string ServicioUso { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Estado { get; set; } = FrecuenciaEstados.Disponible;
    public string Observacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
}
