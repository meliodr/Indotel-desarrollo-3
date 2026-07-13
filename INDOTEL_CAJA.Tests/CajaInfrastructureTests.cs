using INDOTEL_CAJA_REAL_.Clases;
using Xunit;

namespace INDOTEL_CAJA.Tests;

public sealed class PermisosCajaTests
{
    [Theory]
    [InlineData("Administrador")]
    [InlineData("AnalistaDAU")]
    [InlineData("Auditor")]
    public void PuedeIngresar_AceptaRolesInternos(string rol)
    {
        Assert.True(PermisosCaja.PuedeIngresar(rol));
    }

    [Theory]
    [InlineData("Ciudadano")]
    [InlineData("Prestadora")]
    [InlineData("")]
    [InlineData(null)]
    public void PuedeIngresar_RechazaRolesExternos(string rol)
    {
        Assert.False(PermisosCaja.PuedeIngresar(rol));
    }

    [Fact]
    public void Auditor_EsSoloLectura()
    {
        Assert.True(PermisosCaja.SoloLectura("Auditor"));
        Assert.False(PermisosCaja.PuedeGestionar("Auditor"));
    }
}

public sealed class ValidacionesCajaTests
{
    [Theory]
    [InlineData("00100000001")]
    [InlineData("001-0000000-1")]
    public void CedulaValida_AceptaOnceDigitos(string cedula)
    {
        Assert.True(ValidacionesCaja.CedulaValida(cedula));
    }

    [Theory]
    [InlineData("0010000000")]
    [InlineData("0010000000A")]
    [InlineData("")]
    [InlineData(null)]
    public void CedulaValida_RechazaValoresInvalidos(string cedula)
    {
        Assert.False(ValidacionesCaja.CedulaValida(cedula));
    }

    [Fact]
    public void MensajeConReferencia_IncluyeCorrelationId()
    {
        var response = new ApiResponse<object>
        {
            Mensaje = "Servicio no disponible.",
            CorrelationId = "corr-123"
        };

        Assert.Contains("corr-123", response.MensajeConReferencia);
    }
}
