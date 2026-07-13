using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class CerrarReclamacionDto
{
    private string _motivoCierre = string.Empty;
    private string _comentarioCierre = string.Empty;

    [Required]
    [StringLength(500)]
    public string MotivoCierre
    {
        get => _motivoCierre;
        set => _motivoCierre = value ?? string.Empty;
    }

    [StringLength(2000)]
    public string ComentarioCierre
    {
        get => _comentarioCierre;
        set => _comentarioCierre = value ?? string.Empty;
    }

    public bool? ConformidadCiudadano { get; set; }
}
