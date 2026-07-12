using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers
{
    [Authorize]
    public class ReclamacionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ReclamacionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Reclamacion/Crear
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var viewModel = new CrearReclamacionViewModel();

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");

                viewModel.Prestadoras = new List<PrestadoraDto>
                {
                    new PrestadoraDto { Id = 1, NombreComercial = "Claro" },
                    new PrestadoraDto { Id = 2, NombreComercial = "Altice" },
                    new PrestadoraDto { Id = 3, NombreComercial = "Viva" }
                };

                viewModel.Servicios = new List<ServicioTelecomDto>
                {
                    new ServicioTelecomDto { Id = 1, Nombre = "Internet Fijo / Móvil" },
                    new ServicioTelecomDto { Id = 2, Nombre = "Telefonía Móvil" },
                    new ServicioTelecomDto { Id = 3, Nombre = "Televisión por Cable" }
                };
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error al cargar los catálogos del sistema.");
            }

            return View(viewModel);
        }

        // POST: /Reclamacion/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearReclamacionViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");

                var ciudadanoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var token = User.FindFirstValue("JWToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var dataEnviar = new
                {
                    CiudadanoId = int.Parse(ciudadanoId ?? "0"),
                    model.PrestadoraId,
                    model.ServicioId,
                    model.Descripcion
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(dataEnviar), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/reclamaciones", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<CrearReclamacionResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    TempData["NuevoExpediente"] = resultado?.NumeroExpediente ?? "IND-GENERANDO";
                    return RedirectToAction("Exito");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "El Core rechazó la reclamación. Verifique las reglas del caso.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error de comunicación con el servicio central.");
            }

            model.Prestadoras = new List<PrestadoraDto>
            {
                new PrestadoraDto { Id = 1, NombreComercial = "Claro" },
                new PrestadoraDto { Id = 2, NombreComercial = "Altice" }
            };
            model.Servicios = new List<ServicioTelecomDto>
            {
                new ServicioTelecomDto { Id = 1, Nombre = "Internet Fijo / Móvil" },
                new ServicioTelecomDto { Id = 2, Nombre = "Telefonía Móvil" }
            };

            return View(model);
        }

        // GET: /Reclamacion/Detalle/{id}
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var viewModel = new ReclamacionDetalleViewModel();

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var token = User.FindFirstValue("JWToken");

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"/api/reclamaciones/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<ReclamacionDetalleViewModel>(responseString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (resultado != null)
                    {
                        viewModel = resultado;
                    }
                }
                else
                {
                    CargarSimulacionDetalle(viewModel, id);
                }
            }
            catch (Exception)
            {
                CargarSimulacionDetalle(viewModel, id);
                ViewBag.AvisoRed = "Nota: Mostrando datos de simulación local (servidor central desconectado).";
            }

            return View(viewModel);
        }

        // Método auxiliar para no romper la UI en pruebas locales sin el Core activo
        private void CargarSimulacionDetalle(ReclamacionDetalleViewModel model, int id)
        {
            model.Id = id;
            model.NumeroExpediente = $"IND-2026-{id:D4}";
            model.Prestadora = "Claro";
            model.Servicio = "Internet Fijo / Móvil";
            model.Descripcion = "El servicio presenta intermitencias constantes y la velocidad recibida no corresponde al plan contratado.";
            model.Estado = "En Proceso";
            model.FechaCreacion = DateTime.Now.AddDays(-5);

            model.Historial = new List<HistorialEstadoDto>
    {
        new HistorialEstadoDto { Id = 1, Estado = "Abierto", Comentario = "Reclamación recibida formalmente a través del Portal Ciudadano.", FechaCambio = DateTime.Now.AddDays(-5) },
        new HistorialEstadoDto { Id = 2, Estado = "En Proceso", Comentario = "El caso ha sido asignado al departamento de inspección de telecomunicaciones.", FechaCambio = DateTime.Now.AddDays(-3) }
    };

            model.Documentos = new List<DocumentoAnexoDto>
    {
        new DocumentoAnexoDto { Id = 101, NombreArchivo = "Evidencia_Contrato.pdf", TipoDocumento = "Evidencia Ciudadana" },
        new DocumentoAnexoDto { Id = 102, NombreArchivo = "Prueba_Velocidad.png", TipoDocumento = "Evidencia Ciudadana" }
    };
        }

        // GET: /Reclamacion/Exito
        [HttpGet]
        public IActionResult Exito()
        {
            ViewBag.NumeroExpediente = TempData["NuevoExpediente"];
            return View();
        }
    }
}