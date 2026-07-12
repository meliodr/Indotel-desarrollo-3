using System.ComponentModel.DataAnnotations;

namespace INDOTEL.WEB.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Digite un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
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
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Digite un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
