using INDOTEL.WEB.Models;
using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace INDOTEL.WEB.Controllers;

[Authorize(Roles = "Ciudadano")]
public class CiudadanoController : Controller
{
    private const int DashboardPageSize = 10;
    private readonly PortalSessionService _portalSession;

    public CiudadanoController(PortalSessionService portalSession)
    {
        _portalSession = portalSession;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var viewModel = new DashboardViewModel
        {
            NombreCiudadano = User.Identity?.Name ?? "Ciudadano",
            PaginaActual = page,
            TamanoPagina = DashboardPageSize
        };

        try
        {
            var client = await _portalSession.CrearClienteAutorizadoAsync(cancellationToken);
            using var response = await client.GetAsync(
                "/api/reclamaciones",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                var todas = JsonSerializer.Deserialize<List<ReclamacionDto>>(
                    responseString,
                    OpcionesJson()) ?? new List<ReclamacionDto>();

                viewModel.TotalReclamaciones = todas.Count;
                viewModel.TotalPendientes = todas.Count(r => !DashboardViewModel.EsEstadoFinal(r.Estado));
                viewModel.TotalResueltas = todas.Count(r =>
                    r.Estado.Equals("RESUELTA", StringComparison.OrdinalIgnoreCase) ||
                    r.Estado.Equals("CERRADA", StringComparison.OrdinalIgnoreCase));
                viewModel.TotalPaginas = (int)Math.Ceiling(
                    todas.Count / (double)DashboardPageSize);

                if (viewModel.TotalPaginas > 0 && page > viewModel.TotalPaginas)
                {
                    return RedirectToAction(nameof(Index), new { page = viewModel.TotalPaginas });
                }

                viewModel.Reclamaciones = todas
                    .Skip((page - 1) * DashboardPageSize)
                    .Take(DashboardPageSize)
                    .ToList();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }
            else
            {
                ViewBag.ErrorCore = await LeerMensajeApi(response, cancellationToken)
                    ?? "No se pudieron cargar tus reclamaciones en este momento.";
            }
        }
        catch (JsonException)
        {
            ViewBag.ErrorCore = "La respuesta de reclamaciones no pudo ser interpretada.";
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Notificaciones(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var listado = new List<NotificacionDto>();

        try
        {
            var client = await _portalSession.CrearClienteAutorizadoAsync(cancellationToken);
            using var response = await client.GetAsync(
                $"/api/notificaciones?page={page}&pageSize=50",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                var resultado = JsonSerializer.Deserialize<NotificacionesResponseDto>(
                    responseString,
                    OpcionesJson());

                listado = resultado?.Data ?? new List<NotificacionDto>();
                ViewBag.PaginaActual = resultado?.Page ?? page;
                ViewBag.TotalNotificaciones = resultado?.Total ?? listado.Count;
                ViewBag.TamanoPagina = resultado?.PageSize ?? 50;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Auth");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }
            else
            {
                ViewBag.ErrorCore = await LeerMensajeApi(response, cancellationToken)
                    ?? "No se pudieron cargar las notificaciones.";
            }
        }
        catch (JsonException)
        {
            ViewBag.ErrorCore = "La respuesta de notificaciones no pudo ser interpretada.";
        }

        return View(listado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarNotificacionLeida(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            TempData["ErrorNotificacion"] = "La notificación seleccionada no es válida.";
            return RedirectToAction(nameof(Notificaciones));
        }

        var client = await _portalSession.CrearClienteAutorizadoAsync(cancellationToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/notificaciones/{id}/leer");
        using var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessNotificacion"] = "Notificación marcada como leída.";
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return RedirectToAction("Login", "Auth");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return Forbid();
        }
        else
        {
            TempData["ErrorNotificacion"] = await LeerMensajeApi(response, cancellationToken)
                ?? "No fue posible actualizar la notificación.";
        }

        return RedirectToAction(nameof(Notificaciones));
    }

    private static JsonSerializerOptions OpcionesJson() => new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<string?> LeerMensajeApi(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var contenido = await response.Content.ReadAsStringAsync(cancellationToken);
            using var documento = JsonDocument.Parse(contenido);
            return documento.RootElement.TryGetProperty("mensaje", out var mensaje)
                ? mensaje.GetString()
                : documento.RootElement.TryGetProperty("detail", out var detail)
                    ? detail.GetString()
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
