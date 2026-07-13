using System.Collections.Generic;

namespace INDOTEL_CAJA_REAL_.Clases
{
    public sealed class TransicionesReclamacionDto
    {
        public int ReclamacionId { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public List<string> TransicionesPermitidas { get; set; } = new List<string>();
        public bool PuedeCambiarEstado { get; set; }
        public bool RespuestaPrestadoraRequerida { get; set; }
    }
}
