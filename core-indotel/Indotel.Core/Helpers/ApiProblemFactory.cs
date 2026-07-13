using Microsoft.AspNetCore.Mvc;

namespace Indotel.Core.Helpers;

public static class ApiProblemFactory
{
    public static ObjectResult Create(
        HttpContext context,
        int statusCode,
        string codigo,
        string mensaje,
        string? titulo = null)
    {
        var problem = Build(context, statusCode, codigo, mensaje, titulo);
        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static ProblemDetails Build(
        HttpContext context,
        int statusCode,
        string codigo,
        string mensaje,
        string? titulo = null)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo ?? GetTitle(statusCode),
            Detail = mensaje,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Se conserva `mensaje` para no romper los clientes existentes.
        problem.Extensions["mensaje"] = mensaje;
        problem.Extensions["codigo"] = codigo;
        problem.Extensions["correlationId"] = context.TraceIdentifier;
        problem.Extensions["fecha"] = DateTime.UtcNow;

        return problem;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Solicitud no válida",
        StatusCodes.Status401Unauthorized => "Autenticación requerida",
        StatusCodes.Status403Forbidden => "Acceso denegado",
        StatusCodes.Status404NotFound => "Recurso no encontrado",
        StatusCodes.Status408RequestTimeout => "Tiempo de espera agotado",
        StatusCodes.Status409Conflict => "Conflicto de datos",
        StatusCodes.Status423Locked => "Usuario bloqueado",
        StatusCodes.Status429TooManyRequests => "Demasiadas solicitudes",
        StatusCodes.Status503ServiceUnavailable => "Servicio no disponible",
        _ => "Error interno del servidor"
    };
}
