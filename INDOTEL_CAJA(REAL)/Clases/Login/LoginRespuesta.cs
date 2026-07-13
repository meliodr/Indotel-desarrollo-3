using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class LoginRespuesta
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiraEn { get; set; }

        public DateTime RefreshTokenExpiraEn { get; set; }

        public UsuarioSesion Usuario { get; set; }

    }
}
