namespace Indotel.Core.Constants;

public static class ReclamacionEstados
{
    public const string Recibida = "RECIBIDA";
    public const string Validada = "VALIDADA";
    public const string Observada = "OBSERVADA";
    public const string EnviadaAPrestadora = "ENVIADA_A_PRESTADORA";
    public const string RespondidaPorPrestadora = "RESPONDIDA_POR_PRESTADORA";
    public const string EnRevision = "EN_REVISION";
    public const string EnRevisionIndotel = "EN_REVISION_INDOTEL";
    public const string Resuelta = "RESUELTA";
    public const string Cerrada = "CERRADA";
    public const string Rechazada = "RECHAZADA";
    public const string Archivada = "ARCHIVADA";
    public const string Vencida = "VENCIDA";

    public static readonly string[] Todos =
    {
        Recibida,
        Validada,
        Observada,
        EnviadaAPrestadora,
        RespondidaPorPrestadora,
        EnRevision,
        EnRevisionIndotel,
        Resuelta,
        Cerrada,
        Rechazada,
        Archivada,
        Vencida
    };

    public static readonly string[] Finales =
    {
        Cerrada,
        Rechazada,
        Archivada
    };
}
