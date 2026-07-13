using System.Data;
using Indotel.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Filters;

public sealed class TransactionalActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    private readonly IndotelDbContext _db;
    private readonly ILogger<TransactionalActionFilter> _logger;

    public TransactionalActionFilter(
        IndotelDbContext db,
        ILogger<TransactionalActionFilter> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!DebeUsarTransaccion(context.HttpContext) ||
            !_db.Database.IsRelational() ||
            _db.Database.CurrentTransaction is not null)
        {
            await next();
            return;
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            context.HttpContext.RequestAborted);

        var executed = await next();

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogWarning(
                "Transacción revertida por excepción. Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                context.HttpContext.TraceIdentifier);
            return;
        }

        var statusCode = ObtenerStatusCode(executed.Result);
        if (statusCode < StatusCodes.Status400BadRequest)
        {
            await transaction.CommitAsync(context.HttpContext.RequestAborted);
            return;
        }

        await transaction.RollbackAsync(context.HttpContext.RequestAborted);
        _logger.LogInformation(
            "Transacción revertida por respuesta HTTP {StatusCode}. Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
            statusCode,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            context.HttpContext.TraceIdentifier);
    }

    private static bool DebeUsarTransaccion(HttpContext context)
    {
        if (!WriteMethods.Contains(context.Request.Method)) return false;
        if (!context.Request.Path.StartsWithSegments("/api/reclamaciones")) return false;

        // La carga multipart coordina archivo físico y base de datos dentro de su controlador.
        return context.Request.ContentType?.StartsWith(
            "multipart/form-data",
            StringComparison.OrdinalIgnoreCase) != true;
    }

    private static int ObtenerStatusCode(IActionResult? result) => result switch
    {
        ObjectResult objectResult => objectResult.StatusCode ?? StatusCodes.Status200OK,
        ForbidResult => StatusCodes.Status403Forbidden,
        ChallengeResult => StatusCodes.Status401Unauthorized,
        StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
        _ => StatusCodes.Status200OK
    };
}
