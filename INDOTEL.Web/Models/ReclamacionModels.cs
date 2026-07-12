namespace INDOTEL.WEB.Models
{
    // Para cargar el selector de prestadoras desde la API
    public class PrestadoraDto
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
    }

    // Para cargar el selector de servicios desde la API
    public class ServicioTelecomDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    // El formulario de captura que llena el ciudadano
    public class CrearReclamacionViewModel
    {
        public int PrestadoraId { get; set; }
        public int ServicioId { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Listas para llenar los combobox / selectores en la vista
        public List<PrestadoraDto> Prestadoras { get; set; } = new List<PrestadoraDto>();
        public List<ServicioTelecomDto> Servicios { get; set; } = new List<ServicioTelecomDto>();
    }

    // Respuesta que nos dará el Core al guardar (con el número de expediente generado)
    public class CrearReclamacionResponse
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
    }
}