namespace Indotel.Core.DTOs;

public class ReclamacionCreateDto
{
    public int CiudadanoId { get; set; }
    public int PrestadoraId { get; set; }
    public int ServicioTelecomId { get; set; }
    public int? TipoReclamacionId { get; set; }
    public int? MotivoReclamacionId { get; set; }
    public string CanalRecepcion { get; set; } = "WEB";
    public string Prioridad { get; set; } = "MEDIA";
    public string Provincia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
