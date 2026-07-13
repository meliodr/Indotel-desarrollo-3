using System.Net;

namespace INDOTEL.WEB.Services;

public sealed class GatewayUnavailableException : Exception
{
    public GatewayUnavailableException(
        string message,
        string codigo,
        bool timeout,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Codigo = codigo;
        Timeout = timeout;
    }

    public string Codigo { get; }
    public bool Timeout { get; }
}

public sealed class GatewayTransportHandler : DelegatingHandler
{
    private const string CorrelationHeader = "X-Correlation-ID";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GatewayTransportHandler> _logger;

    public GatewayTransportHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<GatewayTransportHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var correlationId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

        request.Headers.Remove(CorrelationHeader);
        request.Headers.TryAddWithoutValidation(CorrelationHeader, correlationId);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues(CorrelationHeader, out var values))
            {
                var returnedCorrelationId = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(returnedCorrelationId))
                {
                    correlationId = returnedCorrelationId;
                    if (httpContext is not null)
                    {
                        httpContext.Response.Headers[CorrelationHeader] = returnedCorrelationId;
                    }
                }
            }

            if (response.StatusCode is HttpStatusCode.BadGateway or
                HttpStatusCode.ServiceUnavailable or
                HttpStatusCode.GatewayTimeout)
            {
                var statusCode = response.StatusCode;
                response.Dispose();

                _logger.LogWarning(
                    "Gateway o Core no disponible. StatusCode={StatusCode} Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                    (int)statusCode,
                    request.Method,
                    request.RequestUri?.PathAndQuery,
                    correlationId);

                throw new GatewayUnavailableException(
                    "El servicio central no está disponible temporalmente.",
                    statusCode == HttpStatusCode.GatewayTimeout
                        ? "GATEWAY_TIMEOUT"
                        : "GATEWAY_NO_DISPONIBLE",
                    timeout: statusCode == HttpStatusCode.GatewayTimeout);
            }

            return response;
        }
        catch (GatewayUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Timeout comunicando con el Gateway. Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                request.Method,
                request.RequestUri?.PathAndQuery,
                correlationId);

            throw new GatewayUnavailableException(
                "El servicio central no respondió dentro del tiempo esperado.",
                "GATEWAY_TIMEOUT",
                timeout: true,
                ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo conectar con el Gateway. Metodo={Metodo} Ruta={Ruta} CorrelationId={CorrelationId}",
                request.Method,
                request.RequestUri?.PathAndQuery,
                correlationId);

            throw new GatewayUnavailableException(
                "No fue posible establecer conexión con el servicio central.",
                "GATEWAY_NO_DISPONIBLE",
                timeout: false,
                ex);
        }
    }
}
