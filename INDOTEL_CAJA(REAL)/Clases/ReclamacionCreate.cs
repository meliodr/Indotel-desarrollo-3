using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ReclamacionCreate
    {
        public int CiudadanoId { get; set; }

        public int PrestadoraId { get; set; }

        public int ServicioTelecomId { get; set; }

        public int? TipoReclamacionId { get; set; }

        public int? MotivoReclamacionId { get; set; }

        public string CanalRecepcion { get; set; } = "CAJA";

        public string Prioridad { get; set; } = "MEDIA";

        public string Provincia { get; set; }

        public string Municipio { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

    }
}
