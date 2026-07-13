using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ServicioReportes
    {
        private readonly ApiClient api;

        public ServicioReportes()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<ReporteResumen>> ObtenerResumen()
        {
            return await api.GetAsync<ReporteResumen>(
                "/api/reportes/resumen");
        }

    }
}
