namespace INDOTEL.WEB.Models
{
    public class ReclamacionApiDto
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public int PrestadoraId { get; set; }
        public int ServicioTelecomId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class HistorialEstadoDto
    {
        public int Id { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public DateTime FechaCambio { get; set; }

        public string Estado => string.IsNullOrWhiteSpace(EstadoNuevo) ? EstadoAnterior : EstadoNuevo;
    }

    public class DocumentoAnexoDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoContenido { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }

        public string TipoDocumento => string.IsNullOrWhiteSpace(TipoContenido)
            ? "Documento"
            : TipoContenido;
    }

    public class ReclamacionDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Prestadora { get; set; } = string.Empty;
        public string Servicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<HistorialEstadoDto> Historial { get; set; } = new();
        public List<DocumentoAnexoDto> Documentos { get; set; } = new();
    }
}
