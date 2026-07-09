namespace Indotel.Core.Constants;

public static class LicenciaTecnicaEstados
{
    public const string Solicitada = "SOLICITADA";
    public const string EnEvaluacionTecnica = "EN_EVALUACION_TECNICA";
    public const string Aprobada = "APROBADA";
    public const string Activa = "ACTIVA";
    public const string PorVencer = "POR_VENCER";
    public const string Vencida = "VENCIDA";
    public const string Cancelada = "CANCELADA";

    public static readonly string[] Todos =
    {
        Solicitada,
        EnEvaluacionTecnica,
        Aprobada,
        Activa,
        PorVencer,
        Vencida,
        Cancelada
    };

    public static string Normalizar(string estado)
    {
        return string.IsNullOrWhiteSpace(estado) ? Solicitada : estado.Trim().ToUpperInvariant();
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
            Solicitada => estadoNuevo is EnEvaluacionTecnica or Cancelada,
            EnEvaluacionTecnica => estadoNuevo is Aprobada or Cancelada,
            Aprobada => estadoNuevo is Activa or Cancelada,
            Activa => estadoNuevo is PorVencer or Vencida or Cancelada,
            PorVencer => estadoNuevo is Vencida or Cancelada or Activa,
            Vencida => estadoNuevo is Cancelada,
            Cancelada => false,
            _ => false
        };
    }
}
