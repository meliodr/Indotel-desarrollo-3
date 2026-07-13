using Indotel.Core.Constants;

namespace Indotel.Core.Services;

public static class ReclamacionEstadoService
{
    private static readonly Dictionary<string, string[]> TransicionesPermitidas = new()
    {
        [ReclamacionEstados.Recibida] = new[]
        {
            ReclamacionEstados.Validada,
            ReclamacionEstados.Observada,
            ReclamacionEstados.Rechazada
        },
        [ReclamacionEstados.Observada] = new[]
        {
            ReclamacionEstados.Recibida,
            ReclamacionEstados.Rechazada,
            ReclamacionEstados.Archivada
        },
        [ReclamacionEstados.Validada] = new[]
        {
            ReclamacionEstados.EnviadaAPrestadora,
            ReclamacionEstados.Rechazada
        },
        [ReclamacionEstados.EnviadaAPrestadora] = new[]
        {
            ReclamacionEstados.RespondidaPorPrestadora,
            ReclamacionEstados.Vencida
        },
        [ReclamacionEstados.RespondidaPorPrestadora] = new[]
        {
            ReclamacionEstados.EnRevision,
            ReclamacionEstados.EnRevisionIndotel
        },
        [ReclamacionEstados.EnRevision] = new[]
        {
            ReclamacionEstados.Resuelta,
            ReclamacionEstados.EnviadaAPrestadora,
            ReclamacionEstados.Observada
        },
        [ReclamacionEstados.EnRevisionIndotel] = new[]
        {
            ReclamacionEstados.Resuelta,
            ReclamacionEstados.EnviadaAPrestadora,
            ReclamacionEstados.Observada
        },
        [ReclamacionEstados.Vencida] = new[]
        {
            ReclamacionEstados.EnRevision,
            ReclamacionEstados.EnRevisionIndotel,
            ReclamacionEstados.Resuelta
        },
        [ReclamacionEstados.Resuelta] = new[]
        {
            ReclamacionEstados.Cerrada,
            ReclamacionEstados.Archivada
        },
        [ReclamacionEstados.Cerrada] = Array.Empty<string>(),
        [ReclamacionEstados.Rechazada] = Array.Empty<string>(),
        [ReclamacionEstados.Archivada] = Array.Empty<string>()
    };

    public static string NormalizarEstado(string estado)
    {
        return (estado ?? string.Empty).Trim().ToUpperInvariant();
    }

    public static bool ExisteEstado(string estado)
    {
        var estadoNormalizado = NormalizarEstado(estado);
        return ReclamacionEstados.Todos.Contains(estadoNormalizado);
    }

    public static bool EsFinal(string estado)
    {
        var estadoNormalizado = NormalizarEstado(estado);
        return ReclamacionEstados.Finales.Contains(estadoNormalizado);
    }

    public static bool PuedeCambiar(string estadoActual, string estadoNuevo)
    {
        var actual = NormalizarEstado(estadoActual);
        var nuevo = NormalizarEstado(estadoNuevo);

        return ExisteEstado(actual) &&
               ExisteEstado(nuevo) &&
               TransicionesPermitidas.TryGetValue(actual, out var permitidos) &&
               permitidos.Contains(nuevo);
    }

    public static IReadOnlyList<string> ObtenerTransicionesPermitidas(string estadoActual)
    {
        var actual = NormalizarEstado(estadoActual);

        if (!ExisteEstado(actual) ||
            !TransicionesPermitidas.TryGetValue(actual, out var permitidos))
        {
            return Array.Empty<string>();
        }

        return permitidos.ToArray();
    }

    public static string CrearMensajeTransicionInvalida(string estadoActual, string estadoNuevo)
    {
        return $"No se permite cambiar la reclamacion de {NormalizarEstado(estadoActual)} a {NormalizarEstado(estadoNuevo)}.";
    }
}
