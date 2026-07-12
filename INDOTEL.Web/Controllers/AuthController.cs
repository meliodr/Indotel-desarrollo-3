using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ciudadano");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("/api/auth/login", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var mensajeApi = await LeerMensajeApi(response);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ModelState.AddModelError(string.Empty, mensajeApi ?? "Correo o contraseña incorrectos.");
                    }
                    else if ((int)response.StatusCode == 423)
                    {
                        ModelState.AddModelError(string.Empty, mensajeApi ?? "Usuario bloqueado temporalmente por intentos fallidos.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, mensajeApi ?? "No fue posible iniciar sesión en este momento.");
                    }

                    return View(model);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseString, OpcionesJson());

                if (loginResult is null ||
                    string.IsNullOrWhiteSpace(loginResult.Token) ||
                    string.IsNullOrWhiteSpace(loginResult.RefreshToken) ||
                    loginResult.Usuario is null)
                {
                    ModelState.AddModelError(string.Empty, "La respuesta de autenticación recibida no es válida.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, loginResult.Usuario.Id.ToString()),
                    new(ClaimTypes.Name, loginResult.Usuario.NombreCompleto),
                    new(ClaimTypes.Email, loginResult.Usuario.Correo),
                    new(ClaimTypes.Role, loginResult.Usuario.Rol),
                    new("JWToken", loginResult.Token),
                    new("RefreshToken", loginResult.RefreshToken),
                    new("TokenExpiraEn", loginResult.ExpiraEn.ToUniversalTime().ToString("O")),
                    new("RefreshTokenExpiraEn", loginResult.RefreshTokenExpiraEn.ToUniversalTime().ToString("O"))
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
                };

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Ciudadano");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo establecer conexión con el servicio central.");
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "La respuesta del servicio central no pudo ser interpretada.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Ciudadano");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterCiudadanoRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("/api/auth/register-ciudadano", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Registro completado con éxito. Ya puedes iniciar sesión.";
                    return RedirectToAction(nameof(Login));
                }

                var mensajeApi = await LeerMensajeApi(response);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(string.Empty, mensajeApi ?? "La cédula o el correo ya se encuentran registrados.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError(string.Empty, mensajeApi ?? "Los datos enviados no son válidos.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, mensajeApi ?? "No fue posible completar el registro.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo establecer conexión con el servicio central.");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = User.FindFirstValue("JWToken");
                var refreshToken = User.FindFirstValue("RefreshToken");

                if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    var client = _httpClientFactory.CreateClient("IndotelCore");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var request = new LogoutRequest { RefreshToken = refreshToken };
                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        "application/json");

                    await client.PostAsync("/api/auth/logout", jsonContent);
                }
            }
            catch (HttpRequestException)
            {
                // La sesión local debe cerrarse incluso si el servicio central no responde.
            }
            finally
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToAction(nameof(Login));
        }

        private static JsonSerializerOptions OpcionesJson() => new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static async Task<string?> LeerMensajeApi(HttpResponseMessage response)
        {
            try
            {
                var contenido = await response.Content.ReadAsStringAsync();
                using var documento = JsonDocument.Parse(contenido);

                return documento.RootElement.TryGetProperty("mensaje", out var mensaje)
                    ? mensaje.GetString()
                    : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
