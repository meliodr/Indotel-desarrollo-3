using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Indotel.ApiGateway;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var gatewayOptions = builder.Configuration
    .GetSection("Gateway")
    .Get<GatewayOptions>() ?? new GatewayOptions();

if (!Uri.TryCreate(gatewayOptions.CoreBaseUrl, UriKind.Absolute, out var coreBaseUri) ||
    coreBaseUri.Scheme is not ("http" or "https"))
{
    throw new InvalidOperationException("Gateway:CoreBaseUrl debe ser una URL HTTP o HTTPS válida.");
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = Math.Clamp(
        gatewayOptions.MaxRequestBodyBytes,
        1024 * 1024,
        100L * 1024 * 1024);
});

builder.Services
    .AddOptions<GatewayOptions>()
    .Bind(builder.Configuration.GetSection("Gateway"))
    .Validate(options => Uri.TryCreate(options.CoreBaseUrl, UriKind.Absolute, out _),
        "Gateway:CoreBaseUrl no es válida")
    .ValidateOnStart();

builder.Services.AddSingleton<CoreCircuitBreaker>();
builder.Services.AddSingleton<GatewayMetrics>();
builder.Services.AddSingleton<CoreProxyService>();
builder.Services.AddSingleton<CoreHealthService>();

builder.Services.AddHttpClient("CoreProxy", client =>
{
    client.BaseAddress = coreBaseUri;
    client.Timeout = Timeout.InfiniteTimeSpan;
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    ConnectTimeout = TimeSpan.FromSeconds(
        Math.Clamp(gatewayOptions.ConnectTimeoutSeconds, 1, 30)),
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    AllowAutoRedirect = false
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:5234", "https://localhost:7234")
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.SetIsOriginAllowed(_ => false);
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var group = context.Request.Path.StartsWithSegments("/api/auth")
            ? "auth"
            : context.Request.Path.StartsWithSegments("/api")
                ? "api"
                : "health";

        var authorization = context.Request.Headers.Authorization.ToString();
        var actor = string.IsNullOrWhiteSpace(authorization)
            ? "anon"
            : Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(authorization)))[..16];

        var permitLimit = group == "auth"
            ? Math.Min(20, Math.Max(5, gatewayOptions.RateLimitPermit))
            : Math.Max(20, gatewayOptions.RateLimitPermit);

        return RateLimitPartition.GetFixedWindowLimiter(
            $"{ip}:{actor}:{group}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(
                    Math.Clamp(gatewayOptions.RateLimitWindowSeconds, 1, 3600)),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] =
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await GatewayProblemWriter.WriteAsync(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "LIMITE_SOLICITUDES_EXCEDIDO",
            "Se excedió el límite temporal de solicitudes.",
            cancellationToken);
    };
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("GatewayCors");
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new
{
    servicio = "Indotel.ApiGateway",
    estado = "OK",
    fecha = DateTime.UtcNow
}));

app.MapGet("/health/live", () => Results.Ok(new
{
    servicio = "Indotel.ApiGateway",
    estado = "VIVO",
    fecha = DateTime.UtcNow
}));

app.MapGet("/health/ready", async (
    HttpContext context,
    CoreHealthService healthService,
    CancellationToken cancellationToken) =>
{
    var result = await healthService.CheckAsync(cancellationToken);
    if (result.Disponible)
    {
        await context.Response.WriteAsJsonAsync(new
        {
            servicio = "Indotel.ApiGateway",
            estado = "LISTO",
            core = result.Estado,
            coreStatusCode = result.StatusCode,
            correlationId = context.TraceIdentifier,
            fecha = DateTime.UtcNow
        }, cancellationToken: cancellationToken);
        return;
    }

    await GatewayProblemWriter.WriteAsync(
        context,
        StatusCodes.Status503ServiceUnavailable,
        "CORE_NO_DISPONIBLE",
        "El Gateway está activo, pero el servicio central no está disponible.",
        cancellationToken);
});

app.MapGet("/health/status", (
    CoreCircuitBreaker circuit,
    GatewayMetrics metrics) => Results.Ok(new
{
    servicio = "Indotel.ApiGateway",
    circuito = circuit.Snapshot(),
    metricas = metrics.Snapshot(),
    fecha = DateTime.UtcNow
}));

var proxyMethods = new[]
{
    HttpMethods.Get,
    HttpMethods.Post,
    HttpMethods.Put,
    HttpMethods.Patch,
    HttpMethods.Delete,
    HttpMethods.Options,
    HttpMethods.Head
};

app.MapMethods("/api/{**path}", proxyMethods, async context =>
{
    var proxy = context.RequestServices.GetRequiredService<CoreProxyService>();
    await proxy.ForwardAsync(context);
});

app.MapFallback(async context =>
{
    await GatewayProblemWriter.WriteAsync(
        context,
        StatusCodes.Status404NotFound,
        "RUTA_GATEWAY_NO_ENCONTRADA",
        "La ruta solicitada no existe en el API Gateway.",
        context.RequestAborted);
});

app.Run();

public partial class Program
{
}
