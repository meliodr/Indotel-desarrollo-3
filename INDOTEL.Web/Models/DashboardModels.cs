namespace INDOTEL.WEB.Models
{
    // Para listar las reclamaciones en el panel
    public class ReclamacionDto
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int SlaDias { get; set; }
    }

    // Modelo consolidado para la vista del Dashboard
    public class DashboardViewModel
    {
        public string NombreCiudadano { get; set; } = string.Empty;
        public List<ReclamacionDto> Reclamaciones { get; set; } = new List<ReclamacionDto>();
        public int TotalPendientes => Reclamaciones.Count(r => r.Estado.ToLower() != "resuelto" && r.Estado.ToLower() != "cerrado");
        public int TotalResueltas => Reclamaciones.Count(r => r.Estado.ToLower() == "resuelto");
    }
}