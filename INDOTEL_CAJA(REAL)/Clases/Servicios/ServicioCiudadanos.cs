using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases.Servicios
{
    public class ServicioCiudadanos
    {
        private readonly ApiClient api;

        public ServicioCiudadanos()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<Ciudadano>> BuscarPorCedula(string cedula)
        {
            return await api.GetAsync<Ciudadano>(
                "/api/ciudadanos/cedula/" + cedula);
        }

        public async Task<ApiResponse<List<Ciudadano>>> ObtenerTodos()
        {
            return await api.GetAsync<List<Ciudadano>>(
                "/api/ciudadanos");
        }

        public async Task<ApiResponse<Ciudadano>> Crear(CiudadanoCreate request)
        {
            return await api.PostAsync<Ciudadano>(
                "/api/ciudadanos",
                request);
        }

        public async Task<ApiResponse<List<ReclamacionListado>>> ObtenerReclamaciones(int ciudadanoId)
        {
            return await api.GetAsync<List<ReclamacionListado>>
            (
                "/api/ciudadanos/" + ciudadanoId + "/reclamaciones"
            );
        }
        public async Task<ApiResponse<Ciudadano>> ObtenerPorId(int id)
        {
            return await api.GetAsync<Ciudadano>(
                $"/api/ciudadanos/{id}");
        }

    }
}
