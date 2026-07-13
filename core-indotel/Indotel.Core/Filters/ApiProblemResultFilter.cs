using System.Collections;
using System.Reflection;
using Indotel.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Indotel.Core.Filters;

public sealed class ApiProblemResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!TryGetStatusAndValue(context.Result, out var statusCode, out var value)) return;
        if (statusCode < StatusCodes.Status400BadRequest) return;
        if (value is ProblemDetails) return;

        var mensaje = LeerMensaje(value) ?? GetDefaultMessage(statusCode);
        var codigo = LeerProperty(value, "codigo")?.ToString() ?? GetDefaultCode(statusCode);
        var problem = ApiProblemFactory.Build(
            context.HttpContext,
            statusCode,
            codigo,
            mensaje);

        CopiarExtensiones(value, problem);

        context.Result = new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool TryGetStatusAndValue(
        IActionResult result,
        out int statusCode,
        out object? value)
    {
        switch (result)
        {
            case ObjectResult objectResult:
                statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
                value = objectResult.Value;
                return true;
            case StatusCodeResult statusCodeResult:
                statusCode = statusCodeResult.StatusCode;
                value = null;
                return true;
            default:
                statusCode = 0;
                value = null;
                return false;
        }
    }

    private static string? LeerMensaje(object? value)
    {
        if (value is null) return null;
        if (value is string text) return text;

        return LeerProperty(value, "mensaje")?.ToString()
               ?? LeerProperty(value, "message")?.ToString()
               ?? LeerProperty(value, "detail")?.ToString();
    }

    private static object? LeerProperty(object value, string propertyName)
    {
        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                if (string.Equals(item.Key?.ToString(), propertyName, StringComparison.OrdinalIgnoreCase))
                    return item.Value;
            }
        }

        var property = value.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(value);
    }

    private static void CopiarExtensiones(object? value, ProblemDetails problem)
    {
        if (value is null || value is string) return;

        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanRead) continue;
            if (property.Name.Equals("mensaje", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Equals("message", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Equals("codigo", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            problem.Extensions[property.Name] = property.GetValue(value);
        }
    }

    private static string GetDefaultCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "SOLICITUD_NO_VALIDA",
        StatusCodes.Status401Unauthorized => "AUTENTICACION_REQUERIDA",
        StatusCodes.Status403Forbidden => "ACCESO_DENEGADO",
        StatusCodes.Status404NotFound => "RECURSO_NO_ENCONTRADO",
        StatusCodes.Status409Conflict => "CONFLICTO_DATOS",
        StatusCodes.Status423Locked => "USUARIO_BLOQUEADO",
        StatusCodes.Status429TooManyRequests => "LIMITE_SOLICITUDES_EXCEDIDO",
        StatusCodes.Status503ServiceUnavailable => "SERVICIO_NO_DISPONIBLE",
        _ => $"HTTP_{statusCode}"
    };

    private static string GetDefaultMessage(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "La solicitud contiene datos no válidos",
        StatusCodes.Status401Unauthorized => "La sesión no es válida o ha expirado",
        StatusCodes.Status403Forbidden => "No tiene permiso para ejecutar esta operación",
        StatusCodes.Status404NotFound => "El recurso solicitado no existe",
        StatusCodes.Status409Conflict => "La operación entra en conflicto con el estado actual",
        StatusCodes.Status423Locked => "El usuario está bloqueado temporalmente",
        StatusCodes.Status429TooManyRequests => "Se excedió el límite temporal de solicitudes",
        StatusCodes.Status503ServiceUnavailable => "El servicio no está disponible temporalmente",
        _ => "La solicitud no pudo completarse"
    };
}
