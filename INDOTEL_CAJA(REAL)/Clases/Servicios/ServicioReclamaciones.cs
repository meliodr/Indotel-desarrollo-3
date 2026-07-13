using INDOTEL_CAJA_REAL_.Clases.Servicios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class ServicioReclamaciones
    {
        private readonly ApiClient _api = new ApiClient();

        public Task<ApiResponse<List<Reclamacion>>> ObtenerTodas() =>
            _api.GetAsync<List<Reclamacion>>("/api/reclamaciones");

        public Task<ApiResponse<Reclamacion>> Crear(ReclamacionCreate request) =>
            _api.PostAsync<Reclamacion>("/api/reclamaciones", request);

        public Task<ApiResponse<Reclamacion>> BuscarExpediente(string expediente) =>
            _api.GetAsync<Reclamacion>(
                "/api/reclamaciones/expediente/" + Uri.EscapeDataString(expediente?.Trim() ?? string.Empty));

        public Task<ApiResponse<List<HistorialReclamacion>>> ObtenerHistorial(int id) =>
            _api.GetAsync<List<HistorialReclamacion>>($"/api/reclamaciones/{id}/historial");

        public async Task<ApiResponse<List<Reclamacion>>> BuscarPorCedula(string cedula)
        {
            var ciudadanos = new ServicioCiudadanos();
            var ciudadano = await ciudadanos.BuscarPorCedula(ValidacionesCaja.NormalizarCedula(cedula));
            if (!ciudadano.Exitoso || ciudadano.Datos == null)
            {
                return new ApiResponse<List<Reclamacion>>
                {
                    Exitoso = false,
                    CodigoEstado = ciudadano.CodigoEstado,
                    Codigo = ciudadano.Codigo,
                    Mensaje = ciudadano.Mensaje,
                    CorrelationId = ciudadano.CorrelationId,
                    EsTemporal = ciudadano.EsTemporal
                };
            }

            var resultado = await Buscar(ciudadanoId: ciudadano.Datos.Id, page: 1, pageSize: 100);
            return new ApiResponse<List<Reclamacion>>
            {
                Exitoso = resultado.Exitoso,
                CodigoEstado = resultado.CodigoEstado,
                Codigo = resultado.Codigo,
                Mensaje = resultado.Mensaje,
                CorrelationId = resultado.CorrelationId,
                EsTemporal = resultado.EsTemporal,
                Datos = resultado.Datos?.Data ?? new List<Reclamacion>()
            };
        }

        public Task<ApiResponse<Reclamacion>> ObtenerPorId(int id) =>
            _api.GetAsync<Reclamacion>($"/api/reclamaciones/{id}");

        public Task<ApiResponse<Reclamacion>> CambiarEstado(
            int id,
            CambiarEstadoReclamacionDto dto) =>
            _api.PatchAsync<Reclamacion>($"/api/reclamaciones/{id}/estado", dto);

        public Task<ApiResponse<Reclamacion>> Resolver(
            int id,
            ResolverReclamacionDto dto) =>
            _api.PostAsync<Reclamacion>($"/api/reclamaciones/{id}/resolver", dto);

        public Task<ApiResponse<TransicionesReclamacionDto>> ObtenerTransiciones(int id) =>
            _api.GetAsync<TransicionesReclamacionDto>($"/api/reclamaciones/{id}/transiciones");

        public Task<ApiResponse<PagedResponse<Reclamacion>>> Buscar(
            string numeroExpediente = null,
            string estado = null,
            int? ciudadanoId = null,
            int page = 1,
            int pageSize = 50)
        {
            var query = $"?page={Math.Max(1, page)}&pageSize={Math.Max(1, Math.Min(200, pageSize))}";

            if (!string.IsNullOrWhiteSpace(numeroExpediente))
            {
                query += "&numeroExpediente=" + Uri.EscapeDataString(numeroExpediente.Trim());
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query += "&estado=" + Uri.EscapeDataString(estado.Trim());
            }

            if (ciudadanoId.HasValue)
            {
                query += "&ciudadanoId=" + ciudadanoId.Value;
            }

            return _api.GetAsync<PagedResponse<Reclamacion>>("/api/reclamaciones/buscar" + query);
        }
    }
}
