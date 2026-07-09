namespace Indotel.Core.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioSesionDto Usuario { get; set; } = new();
}
