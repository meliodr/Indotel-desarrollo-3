using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioMotivos
    {
        private readonly ApiClient api = new ApiClient();

        public async Task<ApiResponse<List<MotivoReclamacion>>> ObtenerPorTipo(int tipoId)
        {
            return await api.GetAsync<List<MotivoReclamacion>>(
                "/api/catalogos/reclamaciones/motivos?tipoReclamacionId=" + tipoId);
        }

    }
}
