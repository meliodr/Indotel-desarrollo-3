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

        if (value is ProblemDetails existingProblem)
        {
            NormalizarProblemDetails(context.HttpContext, existingProblem, statusCode);
            context.Result = new ObjectResult(existingProblem)
            {
                StatusCode = statusCode,
                ContentTypes = { "application/problem+json" }
            };
            return;
        }

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

    private static void NormalizarProblemDetails(
        HttpContext context,
        ProblemDetails problem,
        int statusCode)
    {
        problem.Status ??= statusCode;
        problem.Instance ??= context.Request.Path;
        problem.Title ??= GetDefaultMessage(statusCode);

        var mensaje = string.IsNullOrWhiteSpace(problem.Detail)
            ? problem is ValidationProblemDetails
                ? "Uno o mas campos contienen datos no validos"
                : GetDefaultMessage(statusCode)
            : problem.Detail;

        problem.Detail = mensaje;
        problem.Extensions.TryAdd("mensaje", mensaje);
        problem.Extensions.TryAdd("codigo", GetDefaultCode(statusCode));
        problem.Extensions.TryAdd("correlationId", context.TraceIdentifier);
        problem.Extensions.TryAdd("fecha", DateTime.UtcNow);
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

    private static object? LeerProperty(object? value, string propertyName)
    {
        if (value is null) return null;

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
        StatusCodes.Status400BadRequest => "La solicitud contiene datos no validos",
        StatusCodes.Status401Unauthorized => "La sesion no es valida o ha expirado",
        StatusCodes.Status403Forbidden => "No tiene permiso para ejecutar esta operacion",
        StatusCodes.Status404NotFound => "El recurso solicitado no existe",
        StatusCodes.Status409Conflict => "La operacion entra en conflicto con el estado actual",
        StatusCodes.Status423Locked => "El usuario esta bloqueado temporalmente",
        StatusCodes.Status429TooManyRequests => "Se excedio el limite temporal de solicitudes",
        StatusCodes.Status503ServiceUnavailable => "El servicio no esta disponible temporalmente",
        _ => "La solicitud no pudo completarse"
    };
}
