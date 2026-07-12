namespace INDOTEL.WEB.Models
{
    // Para mapear cada hito en la linea de tiempo del caso
    public class HistorialEstadoDto
    {
        public int Id { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public DateTime FechaCambio { get; set; }
    }

    // Para representar los archivos anexados al caso
    public class DocumentoAnexoDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty; // Ej. Evidencia, Resolución
    }

    // Modelo consolidado para la pantalla de vista a detalle
    public class ReclamacionDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Prestadora { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        // Colecciones para el historial y los documentos
        public List<HistorialEstadoDto> Historial { get; set; } = new List<HistorialEstadoDto>();
        public List<DocumentoAnexoDto> Documentos { get; set; } = new List<DocumentoAnexoDto>();
    }
}