using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class ResolverReclamacionDto
{
    private string _resultadoResolucion = string.Empty;
    private string _comentarioResolucion = string.Empty;
    private string _fundamentoResolucion = string.Empty;
    private string _accionOrdenada = string.Empty;

    [Required]
    [StringLength(100)]
    public string ResultadoResolucion
    {
        get => _resultadoResolucion;
        set => _resultadoResolucion = value ?? string.Empty;
    }

    [Required]
    [StringLength(2000)]
    public string ComentarioResolucion
    {
        get => _comentarioResolucion;
        set => _comentarioResolucion = value ?? string.Empty;
    }

    [StringLength(4000)]
    public string FundamentoResolucion
    {
        get => _fundamentoResolucion;
        set => _fundamentoResolucion = value ?? string.Empty;
    }

    [StringLength(2000)]
    public string AccionOrdenada
    {
        get => _accionOrdenada;
        set => _accionOrdenada = value ?? string.Empty;
    }

    [Range(0, 9999999999999999.99)]
    public decimal? MontoAjuste { get; set; }
}
