namespace INDOTEL.WEB.Models
{
    public class ReclamacionDto
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int? DiasHabilesSla { get; set; }
        public DateTime? FechaLimiteRespuesta { get; set; }
    }

    public class DashboardViewModel
    {
        public string NombreCiudadano { get; set; } = string.Empty;
        public List<ReclamacionDto> Reclamaciones { get; set; } = new();

        public int TotalPendientes => Reclamaciones.Count(r => !EsEstadoFinal(r.Estado));

        public int TotalResueltas => Reclamaciones.Count(r =>
            r.Estado.Equals("RESUELTA", StringComparison.OrdinalIgnoreCase) ||
            r.Estado.Equals("CERRADA", StringComparison.OrdinalIgnoreCase));

        private static bool EsEstadoFinal(string estado)
        {
            return estado.Equals("RESUELTA", StringComparison.OrdinalIgnoreCase) ||
                   estado.Equals("CERRADA", StringComparison.OrdinalIgnoreCase) ||
                   estado.Equals("RECHAZADA", StringComparison.OrdinalIgnoreCase) ||
                   estado.Equals("ARCHIVADA", StringComparison.OrdinalIgnoreCase);
        }
    }
}
