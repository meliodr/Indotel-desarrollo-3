namespace Indotel.Core.Models;

public class Reclamacion
{
    public int Id { get; set; }
    public string NumeroExpediente { get; set; } = string.Empty;
    public int CiudadanoId { get; set; }
    public int PrestadoraId { get; set; }
    public int ServicioTelecomId { get; set; }
    public int? TipoReclamacionId { get; set; }
    public int? MotivoReclamacionId { get; set; }
    public string CanalRecepcion { get; set; } = "WEB";
    public string Prioridad { get; set; } = "MEDIA";
    public string Provincia { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "RECIBIDA";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public DateTime? FechaEnvioPrestadora { get; set; }
    public DateTime? FechaLimiteRespuesta { get; set; }
    public DateTime? FechaRespuestaPrestadora { get; set; }
    public int? DiasHabilesSla { get; set; }
    public bool EstaVencida { get; set; }
    public DateTime? FechaMarcadaVencida { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string ResultadoResolucion { get; set; } = string.Empty;
    public string ComentarioResolucion { get; set; } = string.Empty;
    public string FundamentoResolucion { get; set; } = string.Empty;
    public string AccionOrdenada { get; set; } = string.Empty;
    public decimal? MontoAjuste { get; set; }
    public int? UsuarioResolucionId { get; set; }
    public string MotivoCierre { get; set; } = string.Empty;
    public string ComentarioCierre { get; set; } = string.Empty;
    public bool? ConformidadCiudadano { get; set; }
    public int? UsuarioCierreId { get; set; }

    public TipoReclamacion? TipoReclamacion { get; set; }
    public MotivoReclamacion? MotivoReclamacion { get; set; }
}
