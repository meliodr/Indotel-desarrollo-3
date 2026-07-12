using INDOTEL.WEB.Models;
using INDOTEL.WEB.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace INDOTEL.Web.Tests;

public class PortalWebTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PortalWebTests(WebApplicationFactory<Program> factory)
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
    public async Task Inicio_Anonimo_MuestraOpcionesDeAcceso()
    {
        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Iniciar Sesión", html);
        Assert.Contains("Crear una Cuenta", html);
        Assert.DoesNotContain("Ir a Mi Panel", html);
    }

    [Fact]
    public async Task Privacidad_MuestraContenidoRealEnEspanol()
    {
        var response = await _client.GetAsync("/Home/Privacy");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Política de Privacidad", html);
        Assert.Contains("Datos que se registran", html);
        Assert.Contains("Privacidad", html);
    }

    [Fact]
    public async Task PanelCiudadano_SinSesion_RedireccionaAlLogin()
    {
        var response = await _client.GetAsync("/Ciudadano");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/Auth/Login", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public void Registro_ConPasswordCorto_EsInvalido()
    {
        var modelo = new RegisterCiudadanoRequest
        {
            Cedula = "00100000001",
            Nombres = "Prueba",
            Apellidos = "Ciudadana",
            Correo = "prueba@example.com",
            Password = "1234567"
        };

        var resultados = new List<ValidationResult>();
        var esValido = Validator.TryValidateObject(
            modelo,
            new ValidationContext(modelo),
            resultados,
            validateAllProperties: true);

        Assert.False(esValido);
        Assert.Contains(resultados, x => x.MemberNames.Contains(nameof(modelo.Password)));
    }

    [Fact]
    public void Notificacion_ConFechaLectura_SeConsideraLeida()
    {
        var notificacion = new NotificacionDto
        {
            Estado = "PENDIENTE",
            FechaLectura = DateTime.UtcNow
        };

        Assert.True(notificacion.Leida);
    }

    [Fact]
    public void RespuestaLogin_SinRol_NoEsValida()
    {
        var respuesta = new LoginResponse
        {
            Token = "token",
            RefreshToken = "refresh",
            Usuario = new UsuarioDto
            {
                Id = 10,
                Correo = "ciudadano@example.com",
                NombreCompleto = "Ciudadano Prueba",
                Rol = string.Empty
            }
        };

        Assert.False(PortalSessionService.EsRespuestaLoginValida(respuesta));
    }
}
