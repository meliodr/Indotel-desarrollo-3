namespace Indotel.Core.DTOs;

public class ResolverReclamacionDto
{
    public string ResultadoResolucion { get; set; } = string.Empty;
    public string ComentarioResolucion { get; set; } = string.Empty;
    public string FundamentoResolucion { get; set; } = string.Empty;
    public string AccionOrdenada { get; set; } = string.Empty;
    public decimal? MontoAjuste { get; set; }
}
