using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace INDOTEL.WEB.Services
{
    public sealed class PortalSessionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PortalSessionService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext =>
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No existe un contexto HTTP activo.");

        public async Task<HttpClient> CrearClienteAutorizadoAsync(CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("IndotelCore");
            var token = HttpContext.User.FindFirstValue("JWToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return client;
            }

            if (DebeRenovarToken())
            {
                var renovado = await RenovarSesionAsync(cancellationToken);
                if (!renovado)
                {
                    return client;
                }

                token = HttpContext.User.FindFirstValue("JWToken");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task IniciarSesionAsync(LoginResponse loginResult)
        {
            var principal = CrearPrincipal(loginResult);
            var expiraCookie = ConvertirAFechaUtc(loginResult.RefreshTokenExpiraEn);

            if (expiraCookie <= DateTimeOffset.UtcNow)
            {
                expiraCookie = DateTimeOffset.UtcNow.AddHours(1);
            }

            var propiedades = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = expiraCookie
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.User = principal;
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                propiedades);
        }

        public async Task<bool> RenovarSesionAsync(CancellationToken cancellationToken = default)
        {
            var refreshToken = HttpContext.User.FindFirstValue("RefreshToken");
            var refreshExpiraEn = LeerFechaClaim("RefreshTokenExpiraEn");

            if (string.IsNullOrWhiteSpace(refreshToken) ||
                refreshExpiraEn is null ||
                refreshExpiraEn <= DateTimeOffset.UtcNow)
            {
                await CerrarSesionLocalAsync();
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var contenido = new StringContent(
                    JsonSerializer.Serialize(new { RefreshToken = refreshToken }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(
                    "/api/auth/refresh-token",
                    contenido,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await CerrarSesionLocalAsync();
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var loginResult = JsonSerializer.Deserialize<LoginResponse>(json, JsonOptions);

                if (!EsRespuestaLoginValida(loginResult) ||
                    !loginResult!.Usuario.Rol.Equals("Ciudadano", StringComparison.OrdinalIgnoreCase))
                {
                    await CerrarSesionLocalAsync();
                    return false;
                }

                await IniciarSesionAsync(loginResult);
                return true;
            }
            catch (HttpRequestException)
            {
                await CerrarSesionLocalAsync();
                return false;
            }
            catch (JsonException)
            {
                await CerrarSesionLocalAsync();
                return false;
            }
        }

        public async Task CerrarSesionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var client = await CrearClienteAutorizadoAsync(cancellationToken);
                var refreshToken = HttpContext.User.FindFirstValue("RefreshToken");

                if (client.DefaultRequestHeaders.Authorization is not null &&
                    !string.IsNullOrWhiteSpace(refreshToken))
                {
                    var contenido = new StringContent(
                        JsonSerializer.Serialize(new LogoutRequest { RefreshToken = refreshToken }),
                        Encoding.UTF8,
                        "application/json");

                    await client.PostAsync("/api/auth/logout", contenido, cancellationToken);
                }
            }
            catch (HttpRequestException)
            {
                // La sesión local debe cerrarse incluso si la API no responde.
            }
            finally
            {
                await CerrarSesionLocalAsync();
            }
        }

        public async Task RevocarRespuestaLoginAsync(
            LoginResponse loginResult,
            CancellationToken cancellationToken = default)
        {
            if (!EsRespuestaLoginValida(loginResult))
            {
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", loginResult.Token);

                var contenido = new StringContent(
                    JsonSerializer.Serialize(new LogoutRequest
                    {
                        RefreshToken = loginResult.RefreshToken
                    }),
                    Encoding.UTF8,
                    "application/json");

                await client.PostAsync("/api/auth/logout", contenido, cancellationToken);
            }
            catch (HttpRequestException)
            {
                // No se interrumpe el flujo del portal si la revocación remota falla.
            }
        }

        public static bool EsRespuestaLoginValida(LoginResponse? loginResult)
        {
            return loginResult is not null &&
                   loginResult.Usuario is not null &&
                   !string.IsNullOrWhiteSpace(loginResult.Token) &&
                   !string.IsNullOrWhiteSpace(loginResult.RefreshToken) &&
                   loginResult.Usuario.Id > 0 &&
                   !string.IsNullOrWhiteSpace(loginResult.Usuario.Correo) &&
                   !string.IsNullOrWhiteSpace(loginResult.Usuario.Rol);
        }

        private ClaimsPrincipal CrearPrincipal(LoginResponse loginResult)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResult.Usuario.Id.ToString()),
                new(ClaimTypes.Name, loginResult.Usuario.NombreCompleto),
                new(ClaimTypes.Email, loginResult.Usuario.Correo),
                new(ClaimTypes.Role, loginResult.Usuario.Rol),
                new("JWToken", loginResult.Token),
                new("RefreshToken", loginResult.RefreshToken),
                new("TokenExpiraEn", ConvertirAFechaUtc(loginResult.ExpiraEn).ToString("O")),
                new("RefreshTokenExpiraEn", ConvertirAFechaUtc(loginResult.RefreshTokenExpiraEn).ToString("O"))
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identidad);
        }

        private bool DebeRenovarToken()
        {
            var expiraEn = LeerFechaClaim("TokenExpiraEn");
            return expiraEn is null || expiraEn <= DateTimeOffset.UtcNow.AddMinutes(2);
        }

        private DateTimeOffset? LeerFechaClaim(string tipoClaim)
        {
            var valor = HttpContext.User.FindFirstValue(tipoClaim);
            return DateTimeOffset.TryParse(valor, out var fecha) ? fecha.ToUniversalTime() : null;
        }

        private static DateTimeOffset ConvertirAFechaUtc(DateTime fecha)
        {
            var fechaUtc = fecha.Kind == DateTimeKind.Utc
                ? fecha
                : fecha.ToUniversalTime();

            return new DateTimeOffset(fechaUtc);
        }

        private async Task CerrarSesionLocalAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
