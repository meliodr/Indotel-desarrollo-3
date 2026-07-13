using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioTiposReclamacion
    {
        private readonly ApiClient api = new ApiClient();

        public async Task<ApiResponse<List<TipoReclamacion>>> ObtenerTodos()
        {
            return await api.GetAsync<List<TipoReclamacion>>(
                "/api/catalogos/reclamaciones/tipos");
        }

    }
}
