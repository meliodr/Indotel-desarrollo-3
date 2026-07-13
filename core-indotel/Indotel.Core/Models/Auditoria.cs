namespace Indotel.Core.Models;

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string UsuarioCorreo { get; set; } = string.Empty;
    public string UsuarioRol { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Nivel { get; set; } = "INFO";
    public string Detalle { get; set; } = string.Empty;
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string MetodoHttp { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
