using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class Sesion
    {
        public static string Token { get; set; }

        public static string RefreshToken { get; set; }

        public static UsuarioSesion Usuario { get; set; }

        public static bool Logueado
        {
            get
            {
                return !string.IsNullOrEmpty(Token);
            }
        }

        public static void Limpiar()
        {
            Token = "";
            RefreshToken = "";
            Usuario = null;
        }

    }
}
