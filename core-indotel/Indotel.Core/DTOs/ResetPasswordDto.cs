using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class ResetPasswordDto
{
    private string _token = string.Empty;
    private string _passwordNueva = string.Empty;

    [Required]
    public string Token
    {
        get => _token;
        set => _token = value ?? string.Empty;
    }

    [Required]
    [StringLength(200)]
    public string PasswordNueva
    {
        get => _passwordNueva;
        set => _passwordNueva = value ?? string.Empty;
    }
}
