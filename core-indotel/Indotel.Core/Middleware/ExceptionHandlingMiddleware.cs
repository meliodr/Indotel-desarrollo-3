using System.Text.Json;
using Indotel.Core.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly HashSet<int> SqlConflictNumbers = new()
    {
        2601, // Índice único duplicado.
        2627, // Restricción única duplicada.
        547   // Restricción de clave foránea o CHECK.
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            if (!context.Response.HasStarted)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status408RequestTimeout,
                    "SOLICITUD_CANCELADA",
                    "La solicitud fue cancelada o agotó su tiempo de espera");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "Conflicto de concurrencia. CorrelationId={CorrelationId}",
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "CONFLICTO_CONCURRENCIA",
                "El recurso cambió mientras se procesaba la solicitud. Actualice los datos e intente nuevamente.");
        }
        catch (DbUpdateException ex) when (ObtenerSqlException(ex) is { } sqlException && SqlConflictNumbers.Contains(sqlException.Number))
        {
            _logger.LogWarning(
                ex,
                "Conflicto de integridad SQL. Numero={Numero} CorrelationId={CorrelationId}",
                sqlException.Number,
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "CONFLICTO_PERSISTENCIA",
                "La operación entra en conflicto con los datos existentes");
        }
        catch (DbUpdateException ex) when (ObtenerSqlException(ex) is not null)
        {
            _logger.LogError(
                ex,
                "SQL Server no disponible durante persistencia. CorrelationId={CorrelationId}",
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "BASE_DATOS_NO_DISPONIBLE",
                "La base de datos no está disponible temporalmente");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(
                ex,
                "Error de persistencia. CorrelationId={CorrelationId}",
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "CONFLICTO_PERSISTENCIA",
                "La operación entra en conflicto con los datos existentes");
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "SQL Server no disponible. Numero={Numero} CorrelationId={CorrelationId}",
                ex.Number,
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "BASE_DATOS_NO_DISPONIBLE",
                "La base de datos no está disponible temporalmente");
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(
                ex,
                "Dependencia agotó el tiempo de espera. CorrelationId={CorrelationId}",
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "DEPENDENCIA_NO_DISPONIBLE",
                "Una dependencia del Core no respondió dentro del tiempo esperado");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado. CorrelationId={CorrelationId}",
                context.TraceIdentifier);

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "ERROR_INTERNO",
                "Ocurrió un error interno en el servidor");
        }
    }

    private static SqlException? ObtenerSqlException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is SqlException sqlException) return sqlException;
            if (current.InnerException is null) break;
        }

        return null;
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string codigo,
        string mensaje)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;

        var problem = ApiProblemFactory.Build(context, statusCode, codigo, mensaje);
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
