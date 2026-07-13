using Indotel.Core.Services;
using Xunit;

namespace Indotel.Core.Tests;

public sealed class ReclamacionEstadoServiceTests
{
    [Theory]
    [InlineData("RECIBIDA", "VALIDADA")]
    [InlineData("RECIBIDA", "OBSERVADA")]
    [InlineData("VALIDADA", "ENVIADA_A_PRESTADORA")]
    [InlineData("ENVIADA_A_PRESTADORA", "RESPONDIDA_POR_PRESTADORA")]
    [InlineData("RESPONDIDA_POR_PRESTADORA", "EN_REVISION_INDOTEL")]
    [InlineData("EN_REVISION_INDOTEL", "RESUELTA")]
    [InlineData("RESUELTA", "CERRADA")]
    public void PuedeCambiar_AceptaTransicionesValidas(string actual, string nuevo)
    {
        Assert.True(ReclamacionEstadoService.PuedeCambiar(actual, nuevo));
    }

    [Theory]
    [InlineData("RECIBIDA", "CERRADA")]
    [InlineData("CERRADA", "RECIBIDA")]
    [InlineData("RECHAZADA", "VALIDADA")]
    [InlineData("ESTADO_INEXISTENTE", "VALIDADA")]
    public void PuedeCambiar_RechazaTransicionesInvalidas(string actual, string nuevo)
    {
        Assert.False(ReclamacionEstadoService.PuedeCambiar(actual, nuevo));
    }

    [Fact]
    public void ObtenerTransicionesPermitidas_DevuelveRutaCompletaDeRecibida()
    {
        var transiciones = ReclamacionEstadoService.ObtenerTransicionesPermitidas(" recibida ");

        Assert.Contains("VALIDADA", transiciones);
        Assert.Contains("OBSERVADA", transiciones);
        Assert.Contains("RECHAZADA", transiciones);
    }

    [Theory]
    [InlineData("CERRADA")]
    [InlineData("RECHAZADA")]
    [InlineData("ARCHIVADA")]
    public void EstadoFinal_NoPermiteTransiciones(string estado)
    {
        Assert.True(ReclamacionEstadoService.EsFinal(estado));
        Assert.Empty(ReclamacionEstadoService.ObtenerTransicionesPermitidas(estado));
    }
}
