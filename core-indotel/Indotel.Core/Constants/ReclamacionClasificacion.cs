namespace Indotel.Core.Constants;

public static class ReclamacionClasificacion
{
    public static readonly string[] CanalesRecepcion =
    {
        "WEB",
        "CAJA",
        "TELEFONO",
        "CORREO",
        "PRESENCIAL"
    };

    public static readonly string[] Prioridades =
    {
        "BAJA",
        "MEDIA",
        "ALTA",
        "CRITICA"
    };

    public static bool EsCanalValido(string canal)
    {
        return CanalesRecepcion.Contains(Normalizar(canal));
    }

    public static bool EsPrioridadValida(string prioridad)
    {
        return Prioridades.Contains(Normalizar(prioridad));
    }

    public static string Normalizar(string valor)
    {
        return valor.Trim().ToUpperInvariant();
    }
}
