using Indotel.Core.Services;
using Xunit;

namespace Indotel.Core.Tests;

public sealed class FileSignatureValidatorTests
{
    [Fact]
    public async Task PdfValido_EsAceptado()
    {
        await using var stream = new MemoryStream("%PDF-1.7 contenido"u8.ToArray());

        var result = await FileSignatureValidator.ValidarAsync(stream, ".pdf");

        Assert.True(result.Valido);
        Assert.Equal("application/pdf", result.TipoContenido);
    }

    [Fact]
    public async Task ArchivoRenombradoComoPdf_EsRechazado()
    {
        await using var stream = new MemoryStream("contenido que no es pdf"u8.ToArray());

        var result = await FileSignatureValidator.ValidarAsync(stream, ".pdf");

        Assert.False(result.Valido);
    }

    [Fact]
    public async Task PngValido_EsAceptado()
    {
        byte[] bytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };
        await using var stream = new MemoryStream(bytes);

        var result = await FileSignatureValidator.ValidarAsync(stream, ".png");

        Assert.True(result.Valido);
        Assert.Equal("image/png", result.TipoContenido);
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    public async Task JpegValido_EsAceptado(string extension)
    {
        byte[] bytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 };
        await using var stream = new MemoryStream(bytes);

        var result = await FileSignatureValidator.ValidarAsync(stream, extension);

        Assert.True(result.Valido);
        Assert.Equal("image/jpeg", result.TipoContenido);
    }
}
