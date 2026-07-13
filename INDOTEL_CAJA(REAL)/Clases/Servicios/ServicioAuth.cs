using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class ServicioAuth
    {
        private readonly ApiClient _api = new ApiClient();

        public Task<ApiResponse<LoginRespuesta>> Login(LoginRequest request) =>
            _api.PostAsync<LoginRespuesta>("/api/auth/login", request);

        public Task<ApiResponse<object>> Logout() => _api.Logout();
    }
}
