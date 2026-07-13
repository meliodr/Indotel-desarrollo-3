using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class ForgotPasswordDto
{
    private string _correo = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Correo
    {
        get => _correo;
        set => _correo = value ?? string.Empty;
    }
}
