namespace Indotel.Core.Models;

public class MotivoReclamacion
{
    public int Id { get; set; }
    public int TipoReclamacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public TipoReclamacion? TipoReclamacion { get; set; }
}
