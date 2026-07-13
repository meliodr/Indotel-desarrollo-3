using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class LoginRequestDto
{
    private string _correo = string.Empty;
    private string _password = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Correo
    {
        get => _correo;
        set => _correo = value ?? string.Empty;
    }

    [Required]
    [StringLength(200)]
    public string Password
    {
        get => _password;
        set => _password = value ?? string.Empty;
    }
}
