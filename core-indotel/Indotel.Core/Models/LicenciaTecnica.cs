using Indotel.Core.Constants;

namespace Indotel.Core.Models;

public class LicenciaTecnica
{
    public int Id { get; set; }
    public string NumeroLicencia { get; set; } = string.Empty;
    public int? PrestadoraId { get; set; }
    public string EntidadAsignada { get; set; } = string.Empty;
    public int FrecuenciaRadioelectricaId { get; set; }
    public string Estado { get; set; } = LicenciaTecnicaEstados.Solicitada;
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime FechaVencimiento { get; set; } = DateTime.UtcNow.AddYears(1);
    public DateTime? FechaCancelacion { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
    public int? ResolucionInstitucionalId { get; set; }
    public int UsuarioCreacionId { get; set; }
    public int? UsuarioCancelacionId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    public Prestadora? Prestadora { get; set; }
    public FrecuenciaRadioelectrica? FrecuenciaRadioelectrica { get; set; }
    public ResolucionInstitucional? ResolucionInstitucional { get; set; }
}
