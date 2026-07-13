using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ServicioAuth
    {
        private readonly HttpClient _client;

        private readonly ApiClient api;

        public ServicioAuth()
        {
            api = new ApiClient();
        }

        public async Task<ApiResponse<LoginRespuesta>> Login(LoginRequest request)
        {
            return await api.PostAsync<LoginRespuesta>(
                "/api/auth/login",
                request);
        }

        public async Task<ApiResponse<object>> Logout()
        {
            return await api.PostAsync<object>("api/auth/logout", 
                new{
                    refreshToken = Sesion.RefreshToken
                });
        }

    }
}
