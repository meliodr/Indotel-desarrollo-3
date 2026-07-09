namespace Indotel.Core.DTOs;

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}
