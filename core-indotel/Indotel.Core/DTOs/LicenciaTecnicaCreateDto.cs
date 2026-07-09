namespace Indotel.Core.DTOs;

public class LicenciaTecnicaCreateDto
{
    public string NumeroLicencia { get; set; } = string.Empty;
    public int? PrestadoraId { get; set; }
    public string EntidadAsignada { get; set; } = string.Empty;
    public int FrecuenciaRadioelectricaId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? ResolucionInstitucionalId { get; set; }
}
