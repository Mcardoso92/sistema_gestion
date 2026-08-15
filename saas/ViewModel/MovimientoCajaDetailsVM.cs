using saas.Models.Enums;

namespace saas.ViewModel
{
    public class MovimientoCajaDetailsVM
    {
        public int Id { get; set; }

        public string EmpresaNombre { get; set; } = null!;

        public string CajaNombre { get; set; } = null!;

        public TipoMovimientoCaja Tipo { get; set; }

        public DireccionMovimientoCaja Direccion { get; set; }

        public decimal Importe { get; set; }

        public DateTime Fecha { get; set; }

        public string UsuarioNombre { get; set; } = null!;

        public string? MedioPagoNombre { get; set; }

        public int? TurnoCajaId { get; set; }

        public string? CategoriaGastoNombre { get; set; }

        public string? Concepto { get; set; }

        public string? Observaciones { get; set; }

        // Reversión
        public int? MovimientoOrigenId { get; set; }

        public bool EsReversion { get; set; }

        public bool FueRevertido { get; set; }

        public List<MovimientoCajaResumenVM> Reversiones { get; set; }
            = new List<MovimientoCajaResumenVM>();

        // Origen financiero
        public int? CobroVentaId { get; set; }

        public int? PagoProveedorId { get; set; }

        public int? ReintegroVentaId { get; set; }

        public int? ReintegroProveedorId { get; set; }

        public int? TransferenciaCajaId { get; set; }
    }
}