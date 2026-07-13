namespace Indotel.Core.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRevocacion { get; set; }
    public string CreadoPorIp { get; set; } = string.Empty;
    public string RevocadoPorIp { get; set; } = string.Empty;
    public string ReemplazadoPorTokenHash { get; set; } = string.Empty;

    public bool Activo => FechaRevocacion == null && DateTime.UtcNow < ExpiraEn;

    public Usuario? Usuario { get; set; }
}
