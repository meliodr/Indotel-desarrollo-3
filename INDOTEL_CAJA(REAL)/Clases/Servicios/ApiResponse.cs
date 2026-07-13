using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ApiResponse<T>
    {
        public bool Exitoso { get; set; }

        public string Mensaje { get; set; }

        public T Datos { get; set; }

        public int CodigoEstado { get; set; }

    }
}
