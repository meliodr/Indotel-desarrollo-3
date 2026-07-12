using INDOTEL.WEB.Models;
using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers
{
    [Authorize(Roles = "Ciudadano")]
    public class CiudadanoController : Controller
    {
        private readonly PortalSessionService _portalSession;

        public CiudadanoController(PortalSessionService portalSession)
        {
            _portalSession = portalSession;
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
                var client = await _portalSession.CrearClienteAutorizadoAsync();
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
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
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
                var client = await _portalSession.CrearClienteAutorizadoAsync();
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
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNotificacionLeida(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorNotificacion"] = "La notificación seleccionada no es válida.";
                return RedirectToAction(nameof(Notificaciones));
            }

            try
            {
                var client = await _portalSession.CrearClienteAutorizadoAsync();
                using var request = new HttpRequestMessage(
                    HttpMethod.Patch,
                    $"/api/notificaciones/{id}/leer");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessNotificacion"] = "Notificación marcada como leída.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }
                else
                {
                    TempData["ErrorNotificacion"] = "No fue posible actualizar la notificación.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorNotificacion"] = "No se pudo establecer conexión con el servicio central.";
            }

            return RedirectToAction(nameof(Notificaciones));
        }

        private static JsonSerializerOptions OpcionesJson() => new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
