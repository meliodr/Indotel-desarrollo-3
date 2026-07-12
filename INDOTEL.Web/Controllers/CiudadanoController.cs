using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using INDOTEL.WEB.Models;

namespace INDOTEL.WEB.Controllers
{
    //[Authorize] // Protege todo el controlador
    public class CiudadanoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CiudadanoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Ciudadano
        public async Task<IActionResult> Index()
        {
            
            var ciudadanoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nombreCompleto = User.Identity?.Name ?? "Ciudadano";

            var viewModel = new DashboardViewModel
            {
                NombreCiudadano = nombreCompleto,
                Reclamaciones = new List<ReclamacionDto>()
            };

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");

                
                var token = User.FindFirstValue("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                var response = await client.GetAsync($"/api/ciudadanos/{ciudadanoId}/reclamaciones");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var listaReclamaciones = JsonSerializer.Deserialize<List<ReclamacionDto>>(responseString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (listaReclamaciones != null)
                    {
                        viewModel.Reclamaciones = listaReclamaciones;
                    }
                }
            }
            catch (Exception)
            {
                
                ViewBag.ErrorCore = "No se pudieron cargar tus reclamaciones en este momento.";
            }

            return View(viewModel);
        }

        // GET: /Ciudadano/Notificaciones
        [HttpGet]
        public async Task<IActionResult> Notificaciones()
        {
            var listado = new List<NotificacionDto>();

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var token = User.FindFirstValue("JWToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync("/api/notificaciones");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<List<NotificacionDto>>(responseString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (resultado != null)
                    {
                        listado = resultado;
                    }
                }
                else
                {
                    CargarSimulacionNotificaciones(listado);
                }
            }
            catch (Exception)
            {
                CargarSimulacionNotificaciones(listado);
            }

            return View(listado);
        }

        private void CargarSimulacionNotificaciones(List<NotificacionDto> lista)
        {
            lista.Add(new NotificacionDto { Id = 1, Mensaje = "Su reclamación No. IND-2026-0001 ha cambiado de estado a 'En Proceso'.", Fecha = DateTime.Now.AddDays(-1), Leida = false });
            lista.Add(new NotificacionDto { Id = 2, Mensaje = "Bienvenido al nuevo Portal Ciudadano del Sistema Digital INDOTEL.", Fecha = DateTime.Now.AddDays(-5), Leida = true });
        }

    }
}