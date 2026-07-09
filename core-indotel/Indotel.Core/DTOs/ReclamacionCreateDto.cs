namespace Indotel.Core.DTOs;

public class ReclamacionCreateDto
{
    public int CiudadanoId { get; set; }
    public int PrestadoraId { get; set; }
    public int ServicioTelecomId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
