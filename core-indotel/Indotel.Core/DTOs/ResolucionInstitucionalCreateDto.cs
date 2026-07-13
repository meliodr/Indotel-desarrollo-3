namespace Indotel.Core.DTOs;

public class ResolucionInstitucionalCreateDto
{
    public string NumeroResolucion { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public int TipoResolucionId { get; set; }
    public int? ReclamacionId { get; set; }
    public int? PrestadoraId { get; set; }
}
