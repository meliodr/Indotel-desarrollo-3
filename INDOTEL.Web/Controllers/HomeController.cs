using INDOTEL.WEB.Models;
using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace INDOTEL.WEB.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var model = new ErrorViewModel { RequestId = requestId };

        if (feature?.Error is GatewayUnavailableException gatewayError)
        {
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ViewBag.Codigo = gatewayError.Codigo;
            ViewBag.Mensaje = gatewayError.Message;

            _logger.LogWarning(
                gatewayError,
                "Gateway no disponible. RutaOriginal={RutaOriginal} CorrelationId={CorrelationId}",
                feature.Path,
                requestId);

            return View("ServiceUnavailable", model);
        }

        Response.StatusCode = StatusCodes.Status500InternalServerError;
        _logger.LogError(
            feature?.Error,
            "Error no controlado en Web. RutaOriginal={RutaOriginal} CorrelationId={CorrelationId}",
            feature?.Path,
            requestId);

        return View(model);
    }

    [HttpGet("/Home/HttpStatus/{code:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HttpStatus(int code)
    {
        return CrearVistaEstado(code);
    }

    [NonAction]
    private IActionResult CrearVistaEstado(int code)
    {
        Response.StatusCode = code;
        ViewBag.StatusCode = code;
        ViewBag.Titulo = code switch
        {
            StatusCodes.Status403Forbidden => "Acceso denegado",
            StatusCodes.Status404NotFound => "Página no encontrada",
            StatusCodes.Status500InternalServerError => "Error interno",
            StatusCodes.Status503ServiceUnavailable => "Servicio no disponible",
            _ => "No fue posible completar la solicitud"
        };
        ViewBag.Mensaje = code switch
        {
            StatusCodes.Status403Forbidden => "No tienes permiso para acceder a este recurso.",
            StatusCodes.Status404NotFound => "La página solicitada no existe o fue movida.",
            StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado al procesar la solicitud.",
            StatusCodes.Status503ServiceUnavailable => "El servicio central no está disponible temporalmente.",
            _ => "La solicitud no pudo completarse."
        };

        return View("StatusCode");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult PageNotFound()
    {
        return CrearVistaEstado(StatusCodes.Status404NotFound);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ServiceUnavailable()
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        ViewBag.Codigo = "SERVICIO_NO_DISPONIBLE";
        ViewBag.Mensaje = "El servicio central no está disponible temporalmente.";
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
