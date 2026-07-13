using System;
using System.Net.Mail;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public static class ValidacionesCaja
    {
        public static bool CedulaValida(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return false;
            cedula = cedula.Replace("-", string.Empty).Trim();
            if (cedula.Length != 11) return false;

            foreach (var character in cedula)
            {
                if (!char.IsDigit(character)) return false;
            }

            return true;
        }

        public static bool CorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            try
            {
                var address = new MailAddress(correo.Trim());
                return string.Equals(address.Address, correo.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string NormalizarCedula(string cedula) =>
            (cedula ?? string.Empty).Replace("-", string.Empty).Trim();
    }
}
