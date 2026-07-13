using System;
using System.Configuration;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public static class ConfiguracionApi
    {
        public static Uri BaseAddress
        {
            get
            {
                var configured = ConfigurationManager.AppSettings["ApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(configured))
                {
                    configured = "http://localhost:5185/";
                }

                if (!configured.EndsWith("/", StringComparison.Ordinal))
                {
                    configured += "/";
                }

                if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ConfigurationErrorsException(
                        "ApiBaseUrl debe ser una URL HTTP o HTTPS valida.");
                }

                return uri;
            }
        }

        public static TimeSpan Timeout =>
            TimeSpan.FromSeconds(LeerEntero("ApiTimeoutSeconds", 20, 5, 120));

        public static TimeSpan ConnectTimeout =>
            TimeSpan.FromSeconds(LeerEntero("ApiConnectTimeoutSeconds", 5, 1, 30));

        private static int LeerEntero(string key, int defaultValue, int min, int max)
        {
            var raw = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(raw, out var value))
            {
                value = defaultValue;
            }

            return Math.Max(min, Math.Min(max, value));
        }
    }
}
