using System.ComponentModel.DataAnnotations;

namespace INDOTEL.WEB.Models
{
    public class CiudadanoPerfilDto
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }

    public class PrestadoraDto
    {
        public int Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
    }

    public class ServicioTelecomDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CrearReclamacionViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una prestadora.")]
        public int PrestadoraId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el servicio afectado.")]
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La provincia es obligatoria.")]
        [StringLength(100)]
        public string Provincia { get; set; } = string.Empty;

        [Required(ErrorMessage = "El municipio es obligatorio.")]
        [StringLength(100)]
        public string Municipio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MinLength(10, ErrorMessage = "Explique el inconveniente con al menos 10 caracteres.")]
        [StringLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public List<PrestadoraDto> Prestadoras { get; set; } = new();
        public List<ServicioTelecomDto> Servicios { get; set; } = new();
    }

    public class CrearReclamacionResponse
    {
        public int Id { get; set; }
        public string NumeroExpediente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
