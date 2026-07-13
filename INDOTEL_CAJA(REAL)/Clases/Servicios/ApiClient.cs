using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ApiClient
    {
        private readonly HttpClient http;

        public ApiClient()
        {
            http = new HttpClient();

            http.BaseAddress = new Uri("https://localhost:7085/");

            http.Timeout = TimeSpan.FromSeconds(30);

            ActualizarToken();
        }

        private void ActualizarToken()
        {
            if (!string.IsNullOrEmpty(Sesion.Token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", Sesion.Token);
            }
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            ActualizarToken();

            HttpResponseMessage response = await http.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool renovado = await RenovarSesion();

                if (renovado)
                {
                    ActualizarToken();

                    response = await http.GetAsync(url);
                }
            }

            return await ProcesarRespuesta<T>(response);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string url, object datos)
        {
            ActualizarToken();

            string json = JsonConvert.SerializeObject(datos);

            StringContent contenido =
                new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response =
                await http.PostAsync(url, contenido);

            //if (response.StatusCode == HttpStatusCode.Unauthorized)
            //{
            //    bool renovado = await RenovarSesion();

            //    if (renovado)
            //    {
            //        ActualizarToken();

            //        json = JsonConvert.SerializeObject(datos);

            //        contenido = new StringContent(
            //            json,
            //            Encoding.UTF8,
            //            "application/json");

            //        response = await http.PostAsync(url, contenido);
            //    }
            //}

            //return await ProcesarRespuesta<T>(response);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !url.StartsWith("/api/auth"))
            {
                bool renovado = await RenovarSesion();

                if (renovado)
                {
                    ActualizarToken();

                    json = JsonConvert.SerializeObject(datos);

                    contenido = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                    response = await http.PostAsync(url, contenido);
                }
            }

            return await ProcesarRespuesta<T>(response);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string url, object datos)
        {
            ActualizarToken();

            string json = JsonConvert.SerializeObject(datos);

            StringContent contenido =
                new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response =
                await http.PutAsync(url, contenido);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool renovado = await RenovarSesion();

                if (renovado)
                {
                    ActualizarToken();

                    json = JsonConvert.SerializeObject(datos);

                    contenido = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                    response = await http.PutAsync(url, contenido);
                }
            }

            return await ProcesarRespuesta<T>(response);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string url)
        {
            ActualizarToken();

            HttpResponseMessage response =
                await http.DeleteAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool renovado = await RenovarSesion();

                if (renovado)
                {
                    ActualizarToken();

                    response = await http.DeleteAsync(url);
                }
            }

            return new ApiResponse<bool>()
            {
                Exitoso = response.IsSuccessStatusCode,
                CodigoEstado = (int)response.StatusCode,
                Mensaje = response.ReasonPhrase,
                Datos = response.IsSuccessStatusCode
            };
        }

        public async Task<ApiResponse<T>> PatchAsync<T>(string url, object datos)
        {
            ActualizarToken();

            string json = JsonConvert.SerializeObject(datos);

            HttpRequestMessage request =
                new HttpRequestMessage(new HttpMethod("PATCH"), url);

            request.Content =
                new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response =
                await http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool renovado = await RenovarSesion();

                if (renovado)
                {
                    ActualizarToken();

                    request =
                        new HttpRequestMessage(new HttpMethod("PATCH"), url);

                    request.Content =
                        new StringContent(
                            JsonConvert.SerializeObject(datos),
                            Encoding.UTF8,
                            "application/json");

                    response = await http.SendAsync(request);
                }
            }

            return await ProcesarRespuesta<T>(response);
        }

        private async Task<ApiResponse<T>> ProcesarRespuesta<T>(HttpResponseMessage response)
        {
            ApiResponse<T> resultado = new ApiResponse<T>();

            resultado.CodigoEstado = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                resultado.Exitoso = true;

                if (!string.IsNullOrEmpty(json))
                    resultado.Datos = JsonConvert.DeserializeObject<T>(json);

                return resultado;
            }

            resultado.Exitoso = false;

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    resultado.Mensaje = "La sesión ha expirado.";
                    break;



                default:
                    resultado.Mensaje = "Error del servidor.";
                    break;
            }

            return resultado;
        }

        private async Task<bool> RenovarSesion()
        {
            ServicioRefresh servicio = new ServicioRefresh();

            LoginRespuesta login =
                await servicio.RenovarToken();

            if (login == null)
            {
                Sesion.Limpiar();
                return false;
            }

            Sesion.Token = login.Token;

            Sesion.RefreshToken = login.RefreshToken;

            Sesion.Usuario = login.Usuario;

            return true;
        }

        public async Task Logout()
        {
            //var api = new ApiClient();

            await PostAsync<object>(
                "/api/auth/logout",
                new RefreshTokenRequest
                {
                    RefreshToken = Sesion.RefreshToken
                });

            Sesion.Limpiar();
        }



    }
}
