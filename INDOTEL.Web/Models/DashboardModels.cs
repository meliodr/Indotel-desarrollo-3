namespace INDOTEL.WEB.Models;

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
    public int TotalReclamaciones { get; set; }
    public int TotalPendientes { get; set; }
    public int TotalResueltas { get; set; }
    public int PaginaActual { get; set; } = 1;
    public int TamanoPagina { get; set; } = 10;
    public int TotalPaginas { get; set; }
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

    public static bool EsEstadoFinal(string estado)
    {
        return estado.Equals("RESUELTA", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("CERRADA", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("RECHAZADA", StringComparison.OrdinalIgnoreCase) ||
               estado.Equals("ARCHIVADA", StringComparison.OrdinalIgnoreCase);
    }
}
