using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class ApiClient
    {
        private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);
        private static readonly HttpClient Http = CrearHttpClient();

        private static HttpClient CrearHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                ConnectTimeout = ConfiguracionApi.ConnectTimeout,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                UseCookies = false,
                AllowAutoRedirect = false
            };

            return new HttpClient(handler)
            {
                BaseAddress = ConfiguracionApi.BaseAddress,
                Timeout = ConfiguracionApi.Timeout
            };
        }

        public Task<ApiResponse<T>> GetAsync<T>(string url) =>
            EnviarAsync<T>(HttpMethod.Get, url, null, permitirRefresh: true);

        public Task<ApiResponse<T>> PostAsync<T>(string url, object datos) =>
            EnviarAsync<T>(HttpMethod.Post, url, datos, permitirRefresh: !EsRutaAuth(url));

        public Task<ApiResponse<T>> PutAsync<T>(string url, object datos) =>
            EnviarAsync<T>(HttpMethod.Put, url, datos, permitirRefresh: true);

        public Task<ApiResponse<T>> PatchAsync<T>(string url, object datos) =>
            EnviarAsync<T>(new HttpMethod("PATCH"), url, datos, permitirRefresh: true);

        public async Task<ApiResponse<bool>> DeleteAsync(string url)
        {
            var response = await EnviarAsync<object>(HttpMethod.Delete, url, null, permitirRefresh: true);
            return new ApiResponse<bool>
            {
                Exitoso = response.Exitoso,
                CodigoEstado = response.CodigoEstado,
                Codigo = response.Codigo,
                Mensaje = response.Mensaje,
                CorrelationId = response.CorrelationId,
                EsTemporal = response.EsTemporal,
                Datos = response.Exitoso
            };
        }

        public async Task<ApiResponse<object>> Logout()
        {
            ApiResponse<object> respuesta = null;
            try
            {
                if (!Sesion.Logueado || string.IsNullOrWhiteSpace(Sesion.RefreshToken))
                {
                    return new ApiResponse<object>
                    {
                        Exitoso = true,
                        CodigoEstado = 200,
                        Mensaje = "La sesion local ya estaba cerrada."
                    };
                }

                respuesta = await EnviarAsync<object>(
                    HttpMethod.Post,
                    "/api/auth/logout",
                    new RefreshTokenRequest { RefreshToken = Sesion.RefreshToken },
                    permitirRefresh: false);

                return respuesta;
            }
            finally
            {
                Sesion.Limpiar();
            }
        }

        public Task<ApiResponse<object>> HealthAsync() =>
            EnviarAsync<object>(HttpMethod.Get, "/health/ready", null, permitirRefresh: false);

        private async Task<ApiResponse<T>> EnviarAsync<T>(
            HttpMethod method,
            string url,
            object datos,
            bool permitirRefresh)
        {
            try
            {
                using var request = CrearRequest(method, url, datos);
                using var response = await Http.SendAsync(
                    request,
                    HttpCompletionOption.ResponseContentRead);

                if (response.StatusCode == HttpStatusCode.Unauthorized &&
                    permitirRefresh &&
                    !EsRutaAuth(url))
                {
                    var renovado = await RenovarSesion();
                    if (renovado)
                    {
                        using var retry = CrearRequest(method, url, datos);
                        using var retryResponse = await Http.SendAsync(
                            retry,
                            HttpCompletionOption.ResponseContentRead);
                        return await ProcesarRespuesta<T>(retryResponse);
                    }
                }

                return await ProcesarRespuesta<T>(response);
            }
            catch (TaskCanceledException ex)
            {
                RegistroLocal.Error($"Timeout {method} {url}", ex);
                return CrearFallo<T>(
                    503,
                    "GATEWAY_TIMEOUT",
                    "El servicio no respondio dentro del tiempo esperado.",
                    temporal: true);
            }
            catch (HttpRequestException ex)
            {
                RegistroLocal.Error($"Conexion {method} {url}", ex);
                return CrearFallo<T>(
                    503,
                    "GATEWAY_NO_DISPONIBLE",
                    "No fue posible conectar con el servicio central. Verifique la red e intente nuevamente.",
                    temporal: true);
            }
            catch (JsonException ex)
            {
                RegistroLocal.Error($"JSON {method} {url}", ex);
                return CrearFallo<T>(
                    502,
                    "RESPUESTA_INVALIDA",
                    "El servicio devolvio una respuesta que no pudo interpretarse.",
                    temporal: true);
            }
            catch (Exception ex)
            {
                RegistroLocal.Error($"Error inesperado {method} {url}", ex);
                return CrearFallo<T>(
                    500,
                    "ERROR_CAJA",
                    "Ocurrio un error inesperado en Caja.",
                    temporal: false);
            }
        }

        private static HttpRequestMessage CrearRequest(HttpMethod method, string url, object datos)
        {
            var normalized = string.IsNullOrWhiteSpace(url)
                ? string.Empty
                : url.TrimStart('/');
            var request = new HttpRequestMessage(method, normalized);
            var correlationId = Guid.NewGuid().ToString("N");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

            if (!string.IsNullOrWhiteSpace(Sesion.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", Sesion.Token);
            }

            if (datos != null)
            {
                var json = JsonConvert.SerializeObject(datos);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }

        private static async Task<ApiResponse<T>> ProcesarRespuesta<T>(HttpResponseMessage response)
        {
            var body = response.Content == null
                ? string.Empty
                : await response.Content.ReadAsStringAsync();
            var correlationId = ObtenerCorrelationId(response, body);

            if (response.IsSuccessStatusCode)
            {
                T data = default;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    data = JsonConvert.DeserializeObject<T>(body);
                }

                return new ApiResponse<T>
                {
                    Exitoso = true,
                    CodigoEstado = (int)response.StatusCode,
                    Datos = data,
                    Mensaje = "Operacion completada.",
                    CorrelationId = correlationId
                };
            }

            var message = LeerPropiedad(body, "mensaje") ??
                          LeerPropiedad(body, "detail") ??
                          LeerPropiedad(body, "title") ??
                          MensajePorEstado(response.StatusCode);
            var code = LeerPropiedad(body, "codigo") ?? CodigoPorEstado(response.StatusCode);

            return new ApiResponse<T>
            {
                Exitoso = false,
                CodigoEstado = (int)response.StatusCode,
                Codigo = code,
                Mensaje = message,
                CorrelationId = correlationId,
                EsTemporal = response.StatusCode == HttpStatusCode.BadGateway ||
                             response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                             response.StatusCode == HttpStatusCode.GatewayTimeout ||
                             response.StatusCode == HttpStatusCode.RequestTimeout ||
                             (int)response.StatusCode == 429
            };
        }

        private static string ObtenerCorrelationId(HttpResponseMessage response, string body)
        {
            if (response.Headers.TryGetValues("X-Correlation-ID", out var values))
            {
                foreach (var value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }

            return LeerPropiedad(body, "correlationId") ?? string.Empty;
        }

        private static string LeerPropiedad(string body, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                var token = JToken.Parse(body);
                return token.Type == JTokenType.Object
                    ? token.Value<string>(propertyName)
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string MensajePorEstado(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Los datos enviados no son validos.",
                HttpStatusCode.Unauthorized => "Las credenciales son incorrectas o la sesion expiro.",
                HttpStatusCode.Forbidden => "No tiene permiso para realizar esta operacion.",
                HttpStatusCode.NotFound => "El recurso solicitado no fue encontrado.",
                HttpStatusCode.Conflict => "La operacion entra en conflicto con el estado actual.",
                HttpStatusCode.RequestTimeout => "La solicitud excedio el tiempo permitido.",
                HttpStatusCode.TooManyRequests => "Se excedio el limite temporal de solicitudes.",
                HttpStatusCode.BadGateway => "El Gateway no pudo comunicarse con el servicio central.",
                HttpStatusCode.ServiceUnavailable => "El servicio central no esta disponible temporalmente.",
                HttpStatusCode.GatewayTimeout => "El servicio central no respondio a tiempo.",
                _ when (int)statusCode == 423 => "El usuario se encuentra bloqueado temporalmente.",
                _ => "No fue posible completar la operacion."
            };
        }

        private static string CodigoPorEstado(HttpStatusCode statusCode) =>
            $"HTTP_{(int)statusCode}";

        private static ApiResponse<T> CrearFallo<T>(
            int status,
            string code,
            string message,
            bool temporal)
        {
            return new ApiResponse<T>
            {
                Exitoso = false,
                CodigoEstado = status,
                Codigo = code,
                Mensaje = message,
                CorrelationId = string.Empty,
                EsTemporal = temporal
            };
        }

        private static bool EsRutaAuth(string url) =>
            (url ?? string.Empty).TrimStart('/').StartsWith(
                "api/auth/",
                StringComparison.OrdinalIgnoreCase);

        private static async Task<bool> RenovarSesion()
        {
            await RefreshLock.WaitAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(Sesion.RefreshToken))
                {
                    Sesion.Limpiar();
                    return false;
                }

                using var request = CrearRequest(
                    HttpMethod.Post,
                    "/api/auth/refresh-token",
                    new RefreshTokenRequest { RefreshToken = Sesion.RefreshToken });
                request.Headers.Authorization = null;

                using var response = await Http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest ||
                        response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Sesion.Limpiar();
                    }

                    return false;
                }

                var body = await response.Content.ReadAsStringAsync();
                var login = JsonConvert.DeserializeObject<LoginRespuesta>(body);
                if (login == null ||
                    string.IsNullOrWhiteSpace(login.Token) ||
                    string.IsNullOrWhiteSpace(login.RefreshToken) ||
                    login.Usuario == null)
                {
                    Sesion.Limpiar();
                    return false;
                }

                Sesion.Token = login.Token;
                Sesion.RefreshToken = login.RefreshToken;
                Sesion.Usuario = login.Usuario;
                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException)
            {
                RegistroLocal.Error("Renovacion de sesion", ex);
                return false;
            }
            finally
            {
                RefreshLock.Release();
            }
        }
    }
}
