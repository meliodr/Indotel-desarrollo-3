namespace Indotel.Core.Services;

public static class FileSignatureValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] PngSignature =
    {
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
    };

    public static async Task<(bool Valido, string TipoContenido)> ValidarAsync(
        Stream stream,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (!stream.CanRead) return (false, string.Empty);

        var posicionOriginal = stream.CanSeek ? stream.Position : 0;
        var buffer = new byte[12];
        var leidos = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        if (stream.CanSeek)
        {
            stream.Position = posicionOriginal;
        }

        extension = extension.Trim().ToLowerInvariant();

        if (extension == ".pdf")
        {
            return (EmpiezaCon(buffer, leidos, PdfSignature), "application/pdf");
        }

        if (extension == ".png")
        {
            return (EmpiezaCon(buffer, leidos, PngSignature), "image/png");
        }

        if (extension is ".jpg" or ".jpeg")
        {
            var valido = leidos >= 3 &&
                         buffer[0] == 0xFF &&
                         buffer[1] == 0xD8 &&
                         buffer[2] == 0xFF;
            return (valido, "image/jpeg");
        }

        return (false, string.Empty);
    }

    private static bool EmpiezaCon(byte[] buffer, int leidos, byte[] firma)
    {
        if (leidos < firma.Length) return false;

        for (var index = 0; index < firma.Length; index++)
        {
            if (buffer[index] != firma[index]) return false;
        }

        return true;
    }
}
