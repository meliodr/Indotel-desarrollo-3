using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class UsuarioSesion
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; } = "";

        public string Correo { get; set; } = "";

        public string Rol { get; set; } = "";

    }
}
