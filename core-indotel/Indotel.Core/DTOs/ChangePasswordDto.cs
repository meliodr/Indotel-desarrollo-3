using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class ChangePasswordDto
{
    private string _passwordActual = string.Empty;
    private string _passwordNueva = string.Empty;

    [Required]
    [StringLength(200)]
    public string PasswordActual
    {
        get => _passwordActual;
        set => _passwordActual = value ?? string.Empty;
    }

    [Required]
    [StringLength(200)]
    public string PasswordNueva
    {
        get => _passwordNueva;
        set => _passwordNueva = value ?? string.Empty;
    }
}
