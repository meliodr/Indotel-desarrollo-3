using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers
{
    [Authorize]
    public class CiudadanoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CiudadanoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                NombreCiudadano = User.Identity?.Name ?? "Ciudadano"
            };

            try
            {
                var client = CrearClienteAutorizado();
                var response = await client.GetAsync("/api/reclamaciones");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    viewModel.Reclamaciones = JsonSerializer.Deserialize<List<ReclamacionDto>>(
                        responseString,
                        OpcionesJson()) ?? new List<ReclamacionDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.ErrorCore = "No se pudieron cargar tus reclamaciones en este momento.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorCore = "No se pudo establecer conexión con el servicio central.";
            }
            catch (JsonException)
            {
                ViewBag.ErrorCore = "La respuesta de reclamaciones no pudo ser interpretada.";
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var listado = new List<NotificacionDto>();

            try
            {
                var client = CrearClienteAutorizado();
                var response = await client.GetAsync("/api/notificaciones?page=1&pageSize=50");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<NotificacionesResponseDto>(
                        responseString,
                        OpcionesJson());

                    listado = resultado?.Data ?? new List<NotificacionDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    ViewBag.ErrorCore = "No se pudieron cargar las notificaciones.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorCore = "No se pudo establecer conexión con el servicio central.";
            }
            catch (JsonException)
            {
                ViewBag.ErrorCore = "La respuesta de notificaciones no pudo ser interpretada.";
            }

            return View(listado);
        }

        private HttpClient CrearClienteAutorizado()
        {
            var client = _httpClientFactory.CreateClient("IndotelCore");
            var token = User.FindFirstValue("JWToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private static JsonSerializerOptions OpcionesJson() => new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
