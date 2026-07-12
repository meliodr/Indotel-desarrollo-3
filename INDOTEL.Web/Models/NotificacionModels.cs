namespace INDOTEL.WEB.Models
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool Leida { get; set; }
    }
}