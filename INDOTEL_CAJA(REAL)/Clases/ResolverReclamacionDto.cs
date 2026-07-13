using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ResolverReclamacionDto
    {
        public string ResultadoResolucion { get; set; }

        public string ComentarioResolucion { get; set; }

        public string FundamentoResolucion { get; set; }

        public string AccionOrdenada { get; set; }

        public decimal? MontoAjuste { get; set; }

    }
}
