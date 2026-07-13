using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioCatalogos
    {
        private readonly ApiClient api;

        public ServicioCatalogos()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<List<TipoReclamacion>>> ObtenerTipos()
        {
            return await api.GetAsync<List<TipoReclamacion>>(
                "/api/catalogos/reclamaciones/tipos");
        }

        public async Task<ApiResponse<List<MotivoReclamacion>>> ObtenerMotivos()
        {
            return await api.GetAsync<List<MotivoReclamacion>>(
                "/api/catalogos/reclamaciones/motivos");
        }

        public async Task<ApiResponse<List<string>>> ObtenerCanales()
        {
            return await api.GetAsync<List<string>>(
                "/api/catalogos/reclamaciones/canales");
        }

        public async Task<ApiResponse<List<string>>> ObtenerPrioridades()
        {
            return await api.GetAsync<List<string>>(
                "/api/catalogos/reclamaciones/prioridades");
        }


    }
}
