namespace Indotel.Core.Models;

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
