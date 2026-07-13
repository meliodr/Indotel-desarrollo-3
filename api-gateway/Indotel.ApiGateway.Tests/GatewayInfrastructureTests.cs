using System.Text.Json;
using Indotel.ApiGateway;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Indotel.ApiGateway.Tests;

public sealed class GatewayRequestPolicyTests
{
    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public void IsSafeMethod_AceptaSoloMetodosIdempotentesDeConsulta(string method)
    {
        Assert.True(GatewayRequestPolicy.IsSafeMethod(method));
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public void IsSafeMethod_RechazaMetodosDeEscritura(string method)
    {
        Assert.False(GatewayRequestPolicy.IsSafeMethod(method));
    }

    [Fact]
    public void CanRetry_NoReintentaUnaConsultaConCuerpo()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.ContentLength = 10;

        Assert.False(GatewayRequestPolicy.CanRetry(context.Request));
    }

    [Fact]
    public void CanRetry_AceptaGetSinCuerpo()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;

        Assert.True(GatewayRequestPolicy.CanRetry(context.Request));
    }
}

public sealed class CoreCircuitBreakerTests
{
    [Fact]
    public void Circuito_AbreAlAlcanzarUmbral()
    {
        var circuit = CreateCircuit(threshold: 2);

        circuit.RecordFailure();
        Assert.True(circuit.TryEnter(out _));

        circuit.RecordFailure();
        Assert.False(circuit.TryEnter(out var retryAfter));
        Assert.True(retryAfter > TimeSpan.Zero);
    }

    [Fact]
    public void Circuito_ExitoReiniciaFallosPrevios()
    {
        var circuit = CreateCircuit(threshold: 2);

        circuit.RecordFailure();
        circuit.RecordSuccess();
        circuit.RecordFailure();

        Assert.True(circuit.TryEnter(out _));
    }

    private static CoreCircuitBreaker CreateCircuit(int threshold)
    {
        return new CoreCircuitBreaker(Options.Create(new GatewayOptions
        {
            CircuitFailureThreshold = threshold,
            CircuitOpenSeconds = 30
        }));
    }
}

public sealed class GatewayProblemWriterTests
{
    [Fact]
    public async Task WriteAsync_ProduceProblemDetailsConCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "prueba-123";
        context.Request.Path = "/api/prueba";
        context.Response.Body = new MemoryStream();

        await GatewayProblemWriter.WriteAsync(
            context,
            StatusCodes.Status503ServiceUnavailable,
            "CORE_NO_DISPONIBLE",
            "Servicio temporalmente no disponible");

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;

        Assert.Equal("CORE_NO_DISPONIBLE", root.GetProperty("codigo").GetString());
        Assert.Equal("prueba-123", root.GetProperty("correlationId").GetString());
        Assert.Equal("Servicio temporalmente no disponible", root.GetProperty("mensaje").GetString());
    }
}

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Middleware_ConservaCorrelationIdValido()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "solicitud-abc-123";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("solicitud-abc-123", context.TraceIdentifier);
    }

    [Fact]
    public async Task Middleware_ReemplazaCorrelationIdInvalido()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "valor con espacios";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.NotEqual("valor con espacios", context.TraceIdentifier);
        Assert.Equal(32, context.TraceIdentifier.Length);
    }
}
