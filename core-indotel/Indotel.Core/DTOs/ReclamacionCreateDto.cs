using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class ReclamacionCreateDto
{
    private string _canalRecepcion = "WEB";
    private string _prioridad = "MEDIA";
    private string _provincia = string.Empty;
    private string _municipio = string.Empty;
    private string _titulo = string.Empty;
    private string _descripcion = string.Empty;

    [Range(1, int.MaxValue)]
    public int CiudadanoId { get; set; }

    [Range(1, int.MaxValue)]
    public int PrestadoraId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServicioTelecomId { get; set; }

    public int? TipoReclamacionId { get; set; }
    public int? MotivoReclamacionId { get; set; }

    [Required]
    [StringLength(30)]
    public string CanalRecepcion
    {
        get => _canalRecepcion;
        set => _canalRecepcion = value ?? string.Empty;
    }

    [Required]
    [StringLength(20)]
    public string Prioridad
    {
        get => _prioridad;
        set => _prioridad = value ?? string.Empty;
    }

    [Required]
    [StringLength(100)]
    public string Provincia
    {
        get => _provincia;
        set => _provincia = value ?? string.Empty;
    }

    [Required]
    [StringLength(100)]
    public string Municipio
    {
        get => _municipio;
        set => _municipio = value ?? string.Empty;
    }

    [Required]
    [StringLength(200)]
    public string Titulo
    {
        get => _titulo;
        set => _titulo = value ?? string.Empty;
    }

    [Required]
    [StringLength(4000)]
    public string Descripcion
    {
        get => _descripcion;
        set => _descripcion = value ?? string.Empty;
    }
}
