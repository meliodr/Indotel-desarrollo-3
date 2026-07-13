namespace Indotel.Core.DTOs;

public class UsuarioCreateDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
}
