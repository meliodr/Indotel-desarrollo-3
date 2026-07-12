using INDOTEL.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var viewModel = new CrearReclamacionViewModel();
            await CargarCatalogos(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearReclamacionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCatalogos(model);
                return View(model);
            }

            try
            {
                var client = CrearClienteAutorizado();
                var perfilResponse = await client.GetAsync("/api/ciudadanos/me");

                if (perfilResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                if (!perfilResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        await LeerMensajeApi(perfilResponse) ?? "No se pudo identificar el perfil ciudadano de la sesión.");
                    await CargarCatalogos(model);
                    return View(model);
                }

                var perfilJson = await perfilResponse.Content.ReadAsStringAsync();
                var perfil = JsonSerializer.Deserialize<CiudadanoPerfilDto>(perfilJson, OpcionesJson());

                if (perfil is null || perfil.Id <= 0)
                {
                    ModelState.AddModelError(string.Empty, "El perfil ciudadano recibido no es válido.");
                    await CargarCatalogos(model);
                    return View(model);
                }

                var dataEnviar = new
                {
                    CiudadanoId = perfil.Id,
                    model.PrestadoraId,
                    ServicioTelecomId = model.ServicioId,
                    TipoReclamacionId = (int?)null,
                    MotivoReclamacionId = (int?)null,
                    CanalRecepcion = "WEB",
                    Prioridad = "MEDIA",
                    model.Provincia,
                    model.Municipio,
                    model.Titulo,
                    model.Descripcion
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(dataEnviar),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("/api/reclamaciones", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<CrearReclamacionResponse>(
                        responseString,
                        OpcionesJson());

                    TempData["NuevoExpediente"] = resultado?.NumeroExpediente ?? "Expediente generado";
                    return RedirectToAction(nameof(Exito));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                ModelState.AddModelError(
                    string.Empty,
                    await LeerMensajeApi(response) ?? "La reclamación fue rechazada por el servicio central.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo establecer conexión con el servicio central.");
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "La respuesta recibida no pudo ser interpretada.");
            }

            await CargarCatalogos(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            try
            {
                var client = CrearClienteAutorizado();
                var response = await client.GetAsync($"/api/reclamaciones/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorCore = await LeerMensajeApi(response) ?? "No se pudo cargar el expediente.";
                    return View(new ReclamacionDetalleViewModel());
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var reclamacion = JsonSerializer.Deserialize<ReclamacionApiDto>(
                    responseString,
                    OpcionesJson());

                if (reclamacion is null)
                {
                    ViewBag.ErrorCore = "El detalle del expediente llegó vacío.";
                    return View(new ReclamacionDetalleViewModel());
                }

                var viewModel = new ReclamacionDetalleViewModel
                {
                    Id = reclamacion.Id,
                    NumeroExpediente = reclamacion.NumeroExpediente,
                    Titulo = reclamacion.Titulo,
                    Descripcion = reclamacion.Descripcion,
                    Estado = reclamacion.Estado,
                    FechaCreacion = reclamacion.FechaCreacion,
                    Prestadora = $"Prestadora #{reclamacion.PrestadoraId}",
                    Servicio = $"Servicio #{reclamacion.ServicioTelecomId}"
                };

                await CompletarNombresCatalogos(
                    client,
                    viewModel,
                    reclamacion.PrestadoraId,
                    reclamacion.ServicioTelecomId);

                var historialResponse = await client.GetAsync($"/api/reclamaciones/{id}/historial");
                if (historialResponse.IsSuccessStatusCode)
                {
                    var historialJson = await historialResponse.Content.ReadAsStringAsync();
                    viewModel.Historial = JsonSerializer.Deserialize<List<HistorialEstadoDto>>(
                        historialJson,
                        OpcionesJson()) ?? new List<HistorialEstadoDto>();
                }

                var documentosResponse = await client.GetAsync($"/api/reclamaciones/{id}/documentos");
                if (documentosResponse.IsSuccessStatusCode)
                {
                    var documentosJson = await documentosResponse.Content.ReadAsStringAsync();
                    viewModel.Documentos = JsonSerializer.Deserialize<List<DocumentoAnexoDto>>(
                        documentosJson,
                        OpcionesJson()) ?? new List<DocumentoAnexoDto>();
                }

                return View(viewModel);
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorCore = "No se pudo establecer conexión con el servicio central.";
            }
            catch (JsonException)
            {
                ViewBag.ErrorCore = "La respuesta del expediente no pudo ser interpretada.";
            }

            return View(new ReclamacionDetalleViewModel { Id = id });
        }

        [HttpGet]
        public async Task<IActionResult> DescargarDocumento(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var client = CrearClienteAutorizado();
            var response = await client.GetAsync($"/api/documentos/{id}/descargar");

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode == System.Net.HttpStatusCode.Forbidden
                    ? Forbid()
                    : NotFound();
            }

            var contenido = await response.Content.ReadAsByteArrayAsync();
            var tipoContenido = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var nombreArchivo = response.Content.Headers.ContentDisposition?.FileNameStar
                                ?? response.Content.Headers.ContentDisposition?.FileName
                                ?? $"documento-{id}";

            nombreArchivo = nombreArchivo.Trim('"');
            return File(contenido, tipoContenido, nombreArchivo);
        }

        [HttpGet]
        public IActionResult Exito()
        {
            ViewBag.NumeroExpediente = TempData["NuevoExpediente"];
            return View();
        }

        private async Task CargarCatalogos(CrearReclamacionViewModel model)
        {
            try
            {
                var client = CrearClienteAutorizado();
                var prestadorasResponse = await client.GetAsync("/api/catalogos/prestadoras");
                var serviciosResponse = await client.GetAsync("/api/catalogos/servicios");

                if (prestadorasResponse.IsSuccessStatusCode)
                {
                    var json = await prestadorasResponse.Content.ReadAsStringAsync();
                    model.Prestadoras = JsonSerializer.Deserialize<List<PrestadoraDto>>(
                        json,
                        OpcionesJson()) ?? new List<PrestadoraDto>();
                }

                if (serviciosResponse.IsSuccessStatusCode)
                {
                    var json = await serviciosResponse.Content.ReadAsStringAsync();
                    model.Servicios = JsonSerializer.Deserialize<List<ServicioTelecomDto>>(
                        json,
                        OpcionesJson()) ?? new List<ServicioTelecomDto>();
                }

                if (!prestadorasResponse.IsSuccessStatusCode || !serviciosResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "No fue posible cargar todos los catálogos del sistema.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudo establecer conexión para cargar los catálogos.");
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "Los catálogos recibidos no pudieron ser interpretados.");
            }
        }

        private async Task CompletarNombresCatalogos(
            HttpClient client,
            ReclamacionDetalleViewModel model,
            int prestadoraId,
            int servicioId)
        {
            var prestadorasResponse = await client.GetAsync("/api/catalogos/prestadoras");
            if (prestadorasResponse.IsSuccessStatusCode)
            {
                var json = await prestadorasResponse.Content.ReadAsStringAsync();
                var prestadoras = JsonSerializer.Deserialize<List<PrestadoraDto>>(json, OpcionesJson());
                model.Prestadora = prestadoras?.FirstOrDefault(x => x.Id == prestadoraId)?.NombreComercial
                                   ?? model.Prestadora;
            }

            var serviciosResponse = await client.GetAsync("/api/catalogos/servicios");
            if (serviciosResponse.IsSuccessStatusCode)
            {
                var json = await serviciosResponse.Content.ReadAsStringAsync();
                var servicios = JsonSerializer.Deserialize<List<ServicioTelecomDto>>(json, OpcionesJson());
                model.Servicio = servicios?.FirstOrDefault(x => x.Id == servicioId)?.Nombre
                                  ?? model.Servicio;
            }
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
