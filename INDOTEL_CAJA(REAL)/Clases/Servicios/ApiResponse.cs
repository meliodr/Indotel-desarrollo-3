namespace INDOTEL_CAJA_REAL_.Clases
{
    public class ApiResponse<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public T Datos { get; set; }
        public int CodigoEstado { get; set; }
        public bool EsTemporal { get; set; }

        public string MensajeConReferencia =>
            string.IsNullOrWhiteSpace(CorrelationId)
                ? Mensaje
                : $"{Mensaje}\nReferencia: {CorrelationId}";
    }
}
