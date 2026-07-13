namespace Indotel.Core.DTOs;

public class AuditoriaCreateDto
{
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Nivel { get; set; } = "INFO";
    public string Detalle { get; set; } = string.Empty;
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
}
