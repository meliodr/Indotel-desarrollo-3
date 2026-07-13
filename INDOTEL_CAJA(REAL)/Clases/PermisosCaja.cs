using System;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public static class PermisosCaja
    {
        public const string Administrador = "Administrador";
        public const string AnalistaDau = "AnalistaDAU";
        public const string Auditor = "Auditor";

        public static bool PuedeIngresar(string rol) =>
            EsRol(rol, Administrador) ||
            EsRol(rol, AnalistaDau) ||
            EsRol(rol, Auditor);

        public static bool PuedeGestionar(string rol) =>
            EsRol(rol, Administrador) || EsRol(rol, AnalistaDau);

        public static bool SoloLectura(string rol) => EsRol(rol, Auditor);

        private static bool EsRol(string rol, string esperado) =>
            string.Equals(rol?.Trim(), esperado, StringComparison.OrdinalIgnoreCase);
    }
}
