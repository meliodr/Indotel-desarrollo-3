using System.Data;
using Indotel.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Middleware;

public sealed class TransactionalRequestMiddleware
{
    private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionalRequestMiddleware> _logger;

    public TransactionalRequestMiddleware(
        RequestDelegate next,
        ILogger<TransactionalRequestMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IndotelDbContext db)
    {
        if (!DebeUsarTransaccion(context) ||
            !db.Database.IsRelational() ||
            db.Database.CurrentTransaction is not null)
        {
            await _next(context);
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            context.RequestAborted);

        try
        {
            await _next(context);

            if (context.Response.StatusCode < StatusCodes.Status400BadRequest)
            {
                await transaction.CommitAsync(context.RequestAborted);
            }
            else
            {
                await transaction.RollbackAsync(context.RequestAborted);
            }
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogWarning(
                "Transacción revertida. Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
            throw;
        }
    }

    private static bool DebeUsarTransaccion(HttpContext context)
    {
        if (!WriteMethods.Contains(context.Request.Method)) return false;
        if (!context.Request.Path.StartsWithSegments("/api/reclamaciones")) return false;

        // La carga multipart también escribe un archivo físico y se controla en su controlador.
        return context.Request.ContentType?.StartsWith(
            "multipart/form-data",
            StringComparison.OrdinalIgnoreCase) != true;
    }
}
