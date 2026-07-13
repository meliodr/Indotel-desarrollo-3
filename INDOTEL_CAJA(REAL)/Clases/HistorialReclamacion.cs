using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class HistorialReclamacion
    {
        public int Id { get; set; }

        public int ReclamacionId { get; set; }

        public string EstadoAnterior { get; set; }

        public string EstadoNuevo { get; set; }

        public string Comentario { get; set; }

        public int? UsuarioId { get; set; }

        public DateTime FechaCambio { get; set; }

    }
}
