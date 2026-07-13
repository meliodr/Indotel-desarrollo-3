using System;
using System.IO;
using System.Text;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public static class RegistroLocal
    {
        private static readonly object Sync = new object();

        public static void Error(string contexto, Exception exception, string correlationId = null)
        {
            try
            {
                var directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "INDOTEL",
                    "Caja",
                    "logs");
                Directory.CreateDirectory(directory);

                var file = Path.Combine(directory, $"caja-{DateTime.UtcNow:yyyy-MM-dd}.log");
                var line = new StringBuilder()
                    .Append(DateTime.UtcNow.ToString("O"))
                    .Append(" | ERROR | ")
                    .Append(contexto)
                    .Append(" | CorrelationId=")
                    .Append(string.IsNullOrWhiteSpace(correlationId) ? "N/A" : correlationId)
                    .Append(" | ")
                    .Append(exception.GetType().Name)
                    .Append(": ")
                    .Append(exception.Message)
                    .AppendLine()
                    .ToString();

                lock (Sync)
                {
                    File.AppendAllText(file, line, Encoding.UTF8);
                }
            }
            catch
            {
                // El registro local nunca debe cerrar la aplicacion.
            }
        }
    }
}
