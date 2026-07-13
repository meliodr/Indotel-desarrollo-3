using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ReclamacionListado
    {
        public int Id { get; set; }

        public string NumeroExpediente { get; set; }

        public string Estado { get; set; }

        public string Prestadora { get; set; }

        public string Servicio { get; set; }

        public string Prioridad { get; set; }

        public DateTime FechaCreacion { get; set; }


    }
}
