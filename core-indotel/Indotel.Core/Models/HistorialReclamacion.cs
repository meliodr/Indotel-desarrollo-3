namespace Indotel.Core.Models;

public class HistorialReclamacion
{
    public int Id { get; set; }
    public int ReclamacionId { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;
}
