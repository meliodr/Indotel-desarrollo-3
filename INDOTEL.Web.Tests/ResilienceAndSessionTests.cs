using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using Xunit;

namespace INDOTEL.Web.Tests;

public sealed class PortalStatusPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PortalStatusPageTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task ServiceUnavailable_Devuelve503YMensajeAmigable()
    {
        var response = await _client.GetAsync("/Home/ServiceUnavailable");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("Servicio temporalmente no disponible", html);
        Assert.Contains("Reintentar", html);
    }

    [Fact]
    public async Task RutaInexistente_DevuelvePagina404()
    {
        var response = await _client.GetAsync("/ruta-que-no-existe");
        var html = await response.Content.ReadAsStringAsync();
        var htmlDecodificado = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Página no encontrada", htmlDecodificado);
    }
}

public sealed class PortalTokenStoreTests
{
    [Fact]
    public async Task Store_GuardaYRecuperaTokensDelLadoServidor()
    {
        var services = new ServiceCollection()
            .AddDistributedMemoryCache()
            .BuildServiceProvider();
        var store = new PortalTokenStore(
            services.GetRequiredService<IDistributedCache>());

        var state = new PortalTokenState
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            AccessTokenExpiraEn = DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshTokenExpiraEn = DateTimeOffset.UtcNow.AddDays(7)
        };

        await store.SaveAsync("session-1", state);
        var recovered = await store.GetAsync("session-1");

        Assert.NotNull(recovered);
        Assert.Equal("access-token", recovered!.AccessToken);
        Assert.Equal("refresh-token", recovered.RefreshToken);
    }

    [Fact]
    public async Task Store_EliminaSesion()
    {
        var services = new ServiceCollection()
            .AddDistributedMemoryCache()
            .BuildServiceProvider();
        var store = new PortalTokenStore(
            services.GetRequiredService<IDistributedCache>());

        await store.SaveAsync("session-2", new PortalTokenState
        {
            AccessToken = "a",
            RefreshToken = "r",
            AccessTokenExpiraEn = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshTokenExpiraEn = DateTimeOffset.UtcNow.AddHours(1)
        });
        await store.RemoveAsync("session-2");

        Assert.Null(await store.GetAsync("session-2"));
    }
}

public sealed class GatewayTransportHandlerTests
{
    [Fact]
    public async Task Respuesta503_SeConvierteEnExcepcionControlada()
    {
        var handler = CreateHandler(new StubHandler(
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))));
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5185")
        };

        var exception = await Assert.ThrowsAsync<GatewayUnavailableException>(
            () => client.GetAsync("/api/health"));

        Assert.Equal("GATEWAY_NO_DISPONIBLE", exception.Codigo);
    }

    [Fact]
    public async Task ErrorDeRed_SeConvierteEnExcepcionControlada()
    {
        var handler = CreateHandler(new StubHandler(
            _ => throw new HttpRequestException("sin conexion")));
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5185")
        };

        var exception = await Assert.ThrowsAsync<GatewayUnavailableException>(
            () => client.GetAsync("/api/health"));

        Assert.Equal("GATEWAY_NO_DISPONIBLE", exception.Codigo);
        Assert.False(exception.Timeout);
    }

    private static GatewayTransportHandler CreateHandler(HttpMessageHandler innerHandler)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "web-test-correlation"
        };
        var accessor = new HttpContextAccessor { HttpContext = context };

        return new GatewayTransportHandler(
            accessor,
            NullLogger<GatewayTransportHandler>.Instance)
        {
            InnerHandler = innerHandler
        };
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _send;

        public StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send)
        {
            _send = send;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => _send(request);
    }
}
