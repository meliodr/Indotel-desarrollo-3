using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class RespuestaPrestadoraCreateDto
{
    private string _respuesta = string.Empty;
    private string _documentoSoporte = string.Empty;

    [Range(1, int.MaxValue)]
    public int PrestadoraId { get; set; }

    [Required]
    [StringLength(4000)]
    public string Respuesta
    {
        get => _respuesta;
        set => _respuesta = value ?? string.Empty;
    }

    [StringLength(500)]
    public string DocumentoSoporte
    {
        get => _documentoSoporte;
        set => _documentoSoporte = value ?? string.Empty;
    }
}
