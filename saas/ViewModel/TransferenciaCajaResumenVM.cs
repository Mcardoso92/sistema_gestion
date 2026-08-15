using saas.Models.Enums;

namespace saas.ViewModel
{
    public class TransferenciaCajaResumenVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string CajaOrigenNombre { get; set; } = null!;

        public string CajaDestinoNombre { get; set; } = null!;

        public decimal Importe { get; set; }

        public string Motivo { get; set; } = null!;

        public string UsuarioNombre { get; set; } = null!;

        public int? TurnoCajaId { get; set; }

        public EstadoTransferenciaCaja Estado { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionNombre { get; set; }

        public string? MotivoAnulacion { get; set; }
    }
}