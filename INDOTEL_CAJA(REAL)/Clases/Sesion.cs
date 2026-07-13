namespace INDOTEL_CAJA_REAL_.Clases
{
    public static class Sesion
    {
        public static string Token { get; set; } = string.Empty;
        public static string RefreshToken { get; set; } = string.Empty;
        public static UsuarioSesion Usuario { get; set; }

        public static bool Logueado =>
            !string.IsNullOrWhiteSpace(Token) && Usuario != null;

        public static void Limpiar()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            Usuario = null;
        }
    }
}
