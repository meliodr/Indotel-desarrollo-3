namespace Indotel.Core.Models;

public class DocumentoReclamacion
{
    public int Id { get; set; }
    public int ReclamacionId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
