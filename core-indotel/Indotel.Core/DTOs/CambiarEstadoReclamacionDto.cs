using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class CambiarEstadoReclamacionDto
{
    private string _estadoNuevo = string.Empty;
    private string _comentario = string.Empty;

    [Required]
    [StringLength(80)]
    public string EstadoNuevo
    {
        get => _estadoNuevo;
        set => _estadoNuevo = value ?? string.Empty;
    }

    [StringLength(2000)]
    public string Comentario
    {
        get => _comentario;
        set => _comentario = value ?? string.Empty;
    }
}
