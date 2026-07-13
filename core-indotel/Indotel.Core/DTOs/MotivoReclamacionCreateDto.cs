namespace Indotel.Core.DTOs;

public class MotivoReclamacionCreateDto
{
    public int TipoReclamacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
