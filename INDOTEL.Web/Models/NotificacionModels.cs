namespace INDOTEL.WEB.Models
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public int? ReclamacionId { get; set; }
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }

        public DateTime Fecha => FechaCreacion;
        public bool Leida => FechaLectura.HasValue ||
                             Estado.Equals("LEIDA", StringComparison.OrdinalIgnoreCase);
    }

    public class NotificacionesResponseDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<NotificacionDto> Data { get; set; } = new();
    }
}
