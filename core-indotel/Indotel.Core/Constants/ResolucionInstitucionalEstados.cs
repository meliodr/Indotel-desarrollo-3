namespace Indotel.Core.Constants;

public static class ResolucionInstitucionalEstados
{
    public const string Borrador = "BORRADOR";
    public const string Aprobada = "APROBADA";
    public const string Publicada = "PUBLICADA";
    public const string Archivada = "ARCHIVADA";

    public static readonly string[] Todos =
    {
        Borrador,
        Aprobada,
        Publicada,
        Archivada
    };

    public static string Normalizar(string estado)
    {
        return string.IsNullOrWhiteSpace(estado)
            ? Borrador
            : estado.Trim().ToUpperInvariant();
    }

    public static bool Existe(string estado)
    {
        var normalizado = Normalizar(estado);
        return Todos.Contains(normalizado);
    }

    public static bool PuedeCambiar(string estadoActual, string estadoNuevo)
    {
        estadoActual = Normalizar(estadoActual);
        estadoNuevo = Normalizar(estadoNuevo);

        if (!Existe(estadoActual) || !Existe(estadoNuevo)) return false;
        if (estadoActual == estadoNuevo) return true;

        return estadoActual switch
        {
            Borrador => estadoNuevo is Aprobada or Archivada,
            Aprobada => estadoNuevo is Publicada or Archivada,
            Publicada => estadoNuevo is Archivada,
            Archivada => false,
            _ => false
        };
    }
}
