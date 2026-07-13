using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioServicios
    {
        private readonly ApiClient api;

        public ServicioServicios()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<List<ServicioTelecoms>>> ObtenerTodos()
        {
            return await api.GetAsync<List<ServicioTelecoms>>(
                "/api/servicios");
        }

        public async Task<ApiResponse<ServicioTelecoms>> ObtenerPorId(int id)
        {
            return await api.GetAsync<ServicioTelecoms>(
                $"/api/servicios/{id}");
        }


    }
}
