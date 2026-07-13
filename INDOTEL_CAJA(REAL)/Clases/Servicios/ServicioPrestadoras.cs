using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioPrestadoras
    {
        private readonly ApiClient api = new ApiClient();

        public async Task<ApiResponse<List<Prestadora>>> ObtenerTodas()
        {
            return await api.GetAsync<List<Prestadora>>(
                "/api/prestadoras");
        }

        public async Task<ApiResponse<Prestadora>> ObtenerPorId(int id)
        {
            return await api.GetAsync<Prestadora>(
                $"/api/prestadoras/{id}");
        }

    }
}
