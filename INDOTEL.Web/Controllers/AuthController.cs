using INDOTEL.WEB.Models;
using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PortalSessionService _portalSession;

        public AuthController(
            IHttpClientFactory httpClientFactory,
            PortalSessionService portalSession)
        {
            _httpClientFactory = httpClientFactory;
            _portalSession = portalSession;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole("Ciudadano")
                    ? RedirectToAction("Index", "Ciudadano")
                    : RedirectToAction(nameof(AccessDenied));
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

                if (!PortalSessionService.EsRespuestaLoginValida(loginResult))
                {
                    ModelState.AddModelError(string.Empty, "La respuesta de autenticación recibida no es válida.");
                    return View(model);
                }

                if (!loginResult!.Usuario.Rol.Equals("Ciudadano", StringComparison.OrdinalIgnoreCase))
                {
                    await _portalSession.RevocarRespuestaLoginAsync(loginResult);
                    ModelState.AddModelError(
                        string.Empty,
                        "Este portal es exclusivo para ciudadanos. Los perfiles internos deben utilizar su módulo correspondiente.");
                    return View(model);
                }

                await _portalSession.IniciarSesionAsync(loginResult);
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
                return User.IsInRole("Ciudadano")
                    ? RedirectToAction("Index", "Ciudadano")
                    : RedirectToAction(nameof(AccessDenied));
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
                    var responseString = await response.Content.ReadAsStringAsync();
                    var registroResult = JsonSerializer.Deserialize<LoginResponse>(responseString, OpcionesJson());

                    if (PortalSessionService.EsRespuestaLoginValida(registroResult))
                    {
                        await _portalSession.RevocarRespuestaLoginAsync(registroResult!);
                    }

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
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "La respuesta del servicio central no pudo ser interpretada.");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _portalSession.CerrarSesionAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
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
