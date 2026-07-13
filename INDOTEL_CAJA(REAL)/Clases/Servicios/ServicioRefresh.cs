using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ServicioRefresh
    {

        private readonly HttpClient client;

        public ServicioRefresh()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7085/");
        }

        public async Task<LoginRespuesta> RenovarToken()
        {
            RefreshTokenRequest request = new RefreshTokenRequest();

            request.RefreshToken = Sesion.RefreshToken;

            string json = JsonConvert.SerializeObject(request);

            StringContent contenido =
                new StringContent(json,
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response =
                await client.PostAsync("/api/auth/refresh-token", contenido);

            if (!response.IsSuccessStatusCode)
                return null;

            string respuesta =
                await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginRespuesta>(respuesta);
        }

    }
}
