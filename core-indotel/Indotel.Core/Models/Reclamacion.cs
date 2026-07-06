namespace Indotel.Core.Models;

public class Reclamacion
{
    public int Id { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public int CiudadanoId { get; set; }
    public int PrestadoraId { get; set; }
    public int ServicioTelecomId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "RECIBIDA";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
}
