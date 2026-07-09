namespace Indotel.Core.Models;

public class AsignacionFrecuencia
{
    public int Id { get; set; }
    public int FrecuenciaRadioelectricaId { get; set; }
    public int? PrestadoraId { get; set; }
    public string EntidadAsignada { get; set; } = string.Empty;
    public string UsoAutorizado { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }
    public bool Activa { get; set; } = true;
    public string Observacion { get; set; } = string.Empty;
    public int UsuarioAsignacionId { get; set; }

    public FrecuenciaRadioelectrica? FrecuenciaRadioelectrica { get; set; }
    public Prestadora? Prestadora { get; set; }
}
