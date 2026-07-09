namespace Indotel.Core.Constants;

public static class SolicitudInstitucionalEstados
{
    public const string Recibida = "RECIBIDA";
    public const string EnRevision = "EN_REVISION";
    public const string DocumentacionIncompleta = "DOCUMENTACION_INCOMPLETA";
    public const string Aprobada = "APROBADA";
    public const string Rechazada = "RECHAZADA";
    public const string Vencida = "VENCIDA";
    public const string Renovada = "RENOVADA";

    public static readonly string[] Todos =
    {
        Recibida,
        EnRevision,
        DocumentacionIncompleta,
        Aprobada,
        Rechazada,
        Vencida,
        Renovada
    };

    public static string Normalizar(string estado)
    {
        return string.IsNullOrWhiteSpace(estado) ? Recibida : estado.Trim().ToUpperInvariant();
    }

    public static bool Existe(string estado)
    {
        return Todos.Contains(Normalizar(estado));
    }

    public static bool PuedeCambiar(string estadoActual, string estadoNuevo)
    {
        estadoActual = Normalizar(estadoActual);
        estadoNuevo = Normalizar(estadoNuevo);

        if (!Existe(estadoActual) || !Existe(estadoNuevo)) return false;
        if (estadoActual == estadoNuevo) return true;

        return estadoActual switch
        {
            Recibida => estadoNuevo is EnRevision or DocumentacionIncompleta or Rechazada,
            EnRevision => estadoNuevo is DocumentacionIncompleta or Aprobada or Rechazada,
            DocumentacionIncompleta => estadoNuevo is EnRevision or Rechazada,
            Aprobada => estadoNuevo is Vencida or Renovada,
            Vencida => estadoNuevo is Renovada,
            Renovada => estadoNuevo is Vencida,
            Rechazada => false,
            _ => false
        };
    }
}
