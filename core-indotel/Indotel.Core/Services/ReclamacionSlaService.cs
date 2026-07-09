using Indotel.Core.Models;

namespace Indotel.Core.Services;

public static class ReclamacionSlaService
{
    public static DateTime SumarDiasHabiles(DateTime fechaInicial, int diasHabiles)
    {
        var fecha = fechaInicial.Date;
        var diasAgregados = 0;

        while (diasAgregados < diasHabiles)
        {
            fecha = fecha.AddDays(1);

            if (EsDiaHabil(fecha))
            {
                diasAgregados++;
            }
        }

        return fecha.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
    }

    public static bool EstaVencida(Reclamacion reclamacion, DateTime ahoraUtc)
    {
        if (reclamacion.FechaLimiteRespuesta is null) return false;
        if (reclamacion.FechaRespuestaPrestadora is not null) return false;
        if (reclamacion.EstaVencida) return true;

        return ahoraUtc > reclamacion.FechaLimiteRespuesta.Value;
    }

    private static bool EsDiaHabil(DateTime fecha)
    {
        return fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday;
    }
}
