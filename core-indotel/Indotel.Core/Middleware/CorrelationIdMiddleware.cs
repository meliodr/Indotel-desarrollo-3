namespace Indotel.Core.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var recibido = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = EsValido(recibido)
            ? recibido!
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool EsValido(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128) return false;
        return value.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
    }
}
