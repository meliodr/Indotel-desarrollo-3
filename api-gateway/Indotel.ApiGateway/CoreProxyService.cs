using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Indotel.ApiGateway;

public sealed class CoreProxyService
{
    private static readonly HashSet<string> HopByHopHeaders = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GatewayOptions _options;
    private readonly CoreCircuitBreaker _circuit;
    private readonly GatewayMetrics _metrics;
    private readonly ILogger<CoreProxyService> _logger;
    private readonly Uri _coreBaseUri;

    public CoreProxyService(
        IHttpClientFactory httpClientFactory,
        IOptions<GatewayOptions> options,
        CoreCircuitBreaker circuit,
        GatewayMetrics metrics,
        ILogger<CoreProxyService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _circuit = circuit;
        _metrics = metrics;
        _logger = logger;
        _coreBaseUri = new Uri(_options.CoreBaseUrl, UriKind.Absolute);
    }

    public async Task ForwardAsync(HttpContext context)
    {
        _metrics.Started();

        if (!_circuit.TryEnter(out var retryAfter))
        {
            _metrics.CircuitRejected();
            context.Response.Headers["Retry-After"] = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
            await GatewayProblemWriter.WriteAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "CIRCUITO_CORE_ABIERTO",
                "El servicio central no está disponible temporalmente.",
                context.RequestAborted);
            return;
        }

        var canRetry = GatewayRequestPolicy.CanRetry(context.Request);
        var attempts = canRetry
            ? 1 + Math.Clamp(_options.SafeRetryCount, 0, 2)
            : 1;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                using var request = CreateProxyRequest(context);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.RequestTimeoutSeconds, 1, 120)));

                var client = _httpClientFactory.CreateClient("CoreProxy");
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

                if (IsTransportStatus(response.StatusCode))
                {
                    _circuit.RecordFailure();

                    if (attempt < attempts)
                    {
                        _metrics.Retried();
                        await DelayBeforeRetry(context.RequestAborted);
                        continue;
                    }

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        await CopyResponseAsync(context, response);
                    }
                    else
                    {
                        await GatewayProblemWriter.WriteAsync(
                            context,
                            StatusCodes.Status503ServiceUnavailable,
                            "CORE_NO_DISPONIBLE",
                            "El servicio central no está disponible temporalmente.",
                            context.RequestAborted);
                    }

                    _metrics.Failed();
                    return;
                }

                _circuit.RecordSuccess();
                await CopyResponseAsync(context, response);
                _metrics.Succeeded();
                return;
            }
            catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested)
            {
                _circuit.RecordFailure();
                _logger.LogWarning(
                    "Timeout comunicando con Core. Intento={Intento} Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                    attempt,
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                if (attempt < attempts)
                {
                    _metrics.Retried();
                    await DelayBeforeRetry(context.RequestAborted);
                    continue;
                }

                _metrics.Failed();
                await GatewayProblemWriter.WriteAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    "CORE_TIMEOUT",
                    "El servicio central no respondió dentro del tiempo esperado.",
                    context.RequestAborted);
                return;
            }
            catch (HttpRequestException ex)
            {
                _circuit.RecordFailure();
                _logger.LogWarning(
                    ex,
                    "Fallo de conexión con Core. Intento={Intento} Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                    attempt,
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                if (attempt < attempts)
                {
                    _metrics.Retried();
                    await DelayBeforeRetry(context.RequestAborted);
                    continue;
                }

                _metrics.Failed();
                await GatewayProblemWriter.WriteAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    "CORE_NO_DISPONIBLE",
                    "No fue posible establecer conexión con el servicio central.",
                    context.RequestAborted);
                return;
            }
        }
    }

    private HttpRequestMessage CreateProxyRequest(HttpContext context)
    {
        var relativePath = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;
        var targetUri = new Uri(_coreBaseUri, relativePath + context.Request.QueryString);
        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

        var hasBody = context.Request.ContentLength is > 0 ||
                      context.Request.Headers.ContainsKey("Transfer-Encoding");

        if (hasBody)
        {
            request.Content = new StreamContent(context.Request.Body);
        }

        foreach (var header in context.Request.Headers)
        {
            if (HopByHopHeaders.Contains(header.Key) ||
                header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(CorrelationIdMiddleware.HeaderName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) &&
                request.Content is not null)
            {
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        request.Headers.TryAddWithoutValidation(
            CorrelationIdMiddleware.HeaderName,
            context.TraceIdentifier);

        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", remoteIp);
        }

        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", context.Request.Scheme);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", context.Request.Host.Value);

        return request;
    }

    private static async Task CopyResponseAsync(
        HttpContext context,
        HttpResponseMessage response)
    {
        context.Response.StatusCode = (int)response.StatusCode;

        CopyHeaders(response.Headers, context.Response.Headers);
        CopyHeaders(response.Content.Headers, context.Response.Headers);
        context.Response.Headers.Remove("transfer-encoding");

        if (!HttpMethods.IsHead(context.Request.Method) &&
            response.Content.Headers.ContentLength != 0)
        {
            await response.Content.CopyToAsync(
                context.Response.Body,
                context.RequestAborted);
        }
    }

    private static void CopyHeaders(
        HttpHeaders source,
        IHeaderDictionary destination)
    {
        foreach (var header in source)
        {
            if (HopByHopHeaders.Contains(header.Key)) continue;
            destination[header.Key] = header.Value.ToArray();
        }
    }

    private static bool IsTransportStatus(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;

    private Task DelayBeforeRetry(CancellationToken cancellationToken) =>
        Task.Delay(
            Math.Clamp(_options.SafeRetryDelayMilliseconds, 0, 2000),
            cancellationToken);
}

public sealed record CoreHealthResult(bool Disponible, int? StatusCode, string Estado);

public sealed class CoreHealthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GatewayOptions _options;

    public CoreHealthService(
        IHttpClientFactory httpClientFactory,
        IOptions<GatewayOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<CoreHealthResult> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.ConnectTimeoutSeconds, 1, 15)));

            var client = _httpClientFactory.CreateClient("CoreProxy");
            using var response = await client.GetAsync(
                "health/ready",
                HttpCompletionOption.ResponseHeadersRead,
                timeout.Token);

            return response.IsSuccessStatusCode
                ? new CoreHealthResult(true, (int)response.StatusCode, "DISPONIBLE")
                : new CoreHealthResult(false, (int)response.StatusCode, "NO_DISPONIBLE");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CoreHealthResult(false, null, "TIMEOUT");
        }
        catch (HttpRequestException)
        {
            return new CoreHealthResult(false, null, "SIN_CONEXION");
        }
    }
}
