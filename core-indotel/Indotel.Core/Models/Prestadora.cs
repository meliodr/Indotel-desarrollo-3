namespace Indotel.Core.Models;

public class Prestadora
{
    public int Id { get; set; }
    public string Rnc { get; set; } = string.Empty;
    public string NombreComercial { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Representante { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
