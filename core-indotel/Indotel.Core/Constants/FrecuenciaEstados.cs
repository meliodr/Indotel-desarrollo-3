namespace Indotel.Core.Constants;

public static class FrecuenciaEstados
{
    public const string Disponible = "DISPONIBLE";
    public const string Asignada = "ASIGNADA";
    public const string Reservada = "RESERVADA";
    public const string Suspendida = "SUSPENDIDA";

    public static readonly string[] Todos =
    {
        Disponible,
        Asignada,
        Reservada,
        Suspendida
    };

    public static string Normalizar(string estado)
    {
        return string.IsNullOrWhiteSpace(estado) ? Disponible : estado.Trim().ToUpperInvariant();
    }

    public static bool Existe(string estado)
    {
        return Todos.Contains(Normalizar(estado));
    }
}
