using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class ServicioRefresh
    {
        private readonly ApiClient _api = new ApiClient();

        public async Task<LoginRespuesta> RenovarToken()
        {
            var respuesta = await _api.PostAsync<LoginRespuesta>(
                "/api/auth/refresh-token",
                new RefreshTokenRequest
                {
                    RefreshToken = Sesion.RefreshToken
                });

            return respuesta.Exitoso ? respuesta.Datos : null;
        }
    }
}
