using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class Reclamacion
    {
        public int Id { get; set; }

        public string NumeroExpediente { get; set; }

        public string Titulo { get; set; }

        public string Estado { get; set; }

        public string Provincia { get; set; }

        public string Municipio { get; set; }

        public string Prioridad { get; set; }

        public DateTime FechaCreacion { get; set; }

        public bool EstaVencida { get; set; }

        public int CiudadanoId { get; set; }

        public int PrestadoraId { get; set; }

        public int ServicioTelecomId { get; set; }

        public int? TipoReclamacionId { get; set; }

        public int? MotivoReclamacionId { get; set; }

        public string CanalRecepcion { get; set; }

        public string Descripcion { get; set; }

    }
}
