using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ServicioReclamaciones
    {
        private readonly ApiClient api;

        public ServicioReclamaciones()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<List<Reclamacion>>> ObtenerTodas()
        {
            return await api.GetAsync<List<Reclamacion>>(
                "/api/reclamaciones");
        }

        public async Task<ApiResponse<Reclamacion>> Crear(ReclamacionCreate request)
        {
            return await api.PostAsync<Reclamacion>(
                "/api/reclamaciones",
                request);
        }

        public async Task<ApiResponse<Reclamacion>> BuscarExpediente(string expediente)
        {
            return await api.GetAsync<Reclamacion>
            (
                "/api/reclamaciones/expediente/" + expediente
            );
        }

        public async Task<ApiResponse<List<HistorialReclamacion>>> ObtenerHistorial(int id)
        {
            return await api.GetAsync<List<HistorialReclamacion>>(
                "/api/reclamaciones/" + id + "/historial");
        }

        public async Task<ApiResponse<List<Reclamacion>>> BuscarPorCedula(string cedula)
        {
            return await api.GetAsync<List<Reclamacion>>(
                "/api/reclamaciones/cedula/" + cedula);
        }

        public async Task<ApiResponse<Reclamacion>> ObtenerPorId(int id)
        {
            return await api.GetAsync<Reclamacion>(
                $"/api/reclamaciones/{id}");
        }

        public async Task<ApiResponse<Reclamacion>> CambiarEstado(int id,CambiarEstadoReclamacionDto dto)
        {
            return await api.PatchAsync<Reclamacion>(
                "/api/reclamaciones/" + id + "/estado",
                dto);
        }

        public async Task<ApiResponse<Reclamacion>> Resolver(int id,ResolverReclamacionDto dto)
        {
            return await api.PostAsync<Reclamacion>( "/api/reclamaciones/" + id + "/resolver",dto);
        }


    }
}
