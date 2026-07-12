namespace INDOTEL.WEB.Models
{
    // Para enviar credenciales al Core
    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Para recibir la respuesta del Core
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiraEn { get; set; }
        public UsuarioDto Usuario { get; set; } = new UsuarioDto();
    }

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }

    // Para enviar los datos de registro al Core
    public class RegisterCiudadanoRequest
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }


}