namespace Indotel.Core.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public DateTime RefreshTokenExpiraEn { get; set; }
    public UsuarioSesionDto Usuario { get; set; } = new();
}
