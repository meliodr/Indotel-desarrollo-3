using System.ComponentModel.DataAnnotations;

namespace INDOTEL.WEB.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Digite un correo válido.")]
    [StringLength(254, ErrorMessage = "El correo es demasiado largo.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public DateTime RefreshTokenExpiraEn { get; set; }
    public UsuarioDto Usuario { get; set; } = new();
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public class RegisterCiudadanoRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "La cédula debe contener exactamente 11 dígitos.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Digite un correo válido.")]
    [StringLength(254, ErrorMessage = "El correo es demasiado largo.")]
    public string Correo { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9+()\-\s]{0,25}$", ErrorMessage = "El teléfono contiene caracteres no válidos.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La dirección no puede superar 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,128}$",
        ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y carácter especial.")]
    public string Password { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
