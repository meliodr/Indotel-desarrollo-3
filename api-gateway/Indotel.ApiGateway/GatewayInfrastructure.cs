using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Indotel.ApiGateway;

public sealed class GatewayOptions
{
    public string CoreBaseUrl { get; set; } = "http://localhost:5085/";
    public int RequestTimeoutSeconds { get; set; } = 15;
    public int ConnectTimeoutSeconds { get; set; } = 3;
    public int SafeRetryCount { get; set; } = 1;
    public int SafeRetryDelayMilliseconds { get; set; } = 150;
    public int CircuitFailureThreshold { get; set; } = 5;
    public int CircuitOpenSeconds { get; set; } = 30;
    public long MaxRequestBodyBytes { get; set; } = 8 * 1024 * 1024;
    public int RateLimitPermit { get; set; } = 120;
    public int RateLimitWindowSeconds { get; set; } = 60;
}

public static class GatewayProblemWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string codigo,
        string mensaje,
        CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers[CorrelationIdMiddleware.HeaderName] = context.TraceIdentifier;

        var payload = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = mensaje,
            status = statusCode,
            detail = mensaje,
            instance = context.Request.Path.Value,
            mensaje,
            codigo,
            correlationId = context.TraceIdentifier,
            fecha = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload),
            cancellationToken);
    }
}

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
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = EsValido(incoming)
            ? incoming!
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
        return value.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.');
    }
}

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        await _next(context);
    }
}

public sealed class CoreCircuitBreaker
{
    private readonly object _sync = new();
    private readonly GatewayOptions _options;
    private int _consecutiveFailures;
    private DateTimeOffset? _openUntilUtc;

    public CoreCircuitBreaker(IOptions<GatewayOptions> options)
    {
        _options = options.Value;
    }

    public bool TryEnter(out TimeSpan retryAfter)
    {
        lock (_sync)
        {
            if (_openUntilUtc is null)
            {
                retryAfter = TimeSpan.Zero;
                return true;
            }

            var now = DateTimeOffset.UtcNow;
            if (_openUntilUtc <= now)
            {
                _openUntilUtc = null;
                _consecutiveFailures = 0;
                retryAfter = TimeSpan.Zero;
                return true;
            }

            retryAfter = _openUntilUtc.Value - now;
            return false;
        }
    }

    public void RecordSuccess()
    {
        lock (_sync)
        {
            _consecutiveFailures = 0;
            _openUntilUtc = null;
        }
    }

    public void RecordFailure()
    {
        lock (_sync)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= Math.Max(1, _options.CircuitFailureThreshold))
            {
                _openUntilUtc = DateTimeOffset.UtcNow.AddSeconds(
                    Math.Max(1, _options.CircuitOpenSeconds));
            }
        }
    }

    public object Snapshot()
    {
        lock (_sync)
        {
            return new
            {
                estado = _openUntilUtc is not null && _openUntilUtc > DateTimeOffset.UtcNow
                    ? "ABIERTO"
                    : "CERRADO",
                fallosConsecutivos = _consecutiveFailures,
                abiertoHasta = _openUntilUtc
            };
        }
    }
}

public static class GatewayRequestPolicy
{
    public static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) ||
        HttpMethods.IsHead(method) ||
        HttpMethods.IsOptions(method);

    public static bool CanRetry(HttpRequest request)
    {
        if (!IsSafeMethod(request.Method)) return false;
        return request.ContentLength is null or 0;
    }
}

public sealed class GatewayMetrics
{
    private long _total;
    private long _success;
    private long _failed;
    private long _retried;
    private long _circuitRejected;

    public void Started() => Interlocked.Increment(ref _total);
    public void Succeeded() => Interlocked.Increment(ref _success);
    public void Failed() => Interlocked.Increment(ref _failed);
    public void Retried() => Interlocked.Increment(ref _retried);
    public void CircuitRejected() => Interlocked.Increment(ref _circuitRejected);

    public object Snapshot() => new
    {
        total = Interlocked.Read(ref _total),
        correctas = Interlocked.Read(ref _success),
        fallidas = Interlocked.Read(ref _failed),
        reintentos = Interlocked.Read(ref _retried),
        rechazadasPorCircuito = Interlocked.Read(ref _circuitRejected)
    };
}
