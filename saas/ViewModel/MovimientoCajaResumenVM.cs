using saas.Models.Enums;

namespace saas.ViewModel
{
    public class MovimientoCajaResumenVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string CajaNombre { get; set; } = null!;

        public TipoMovimientoCaja Tipo { get; set; }

        public DireccionMovimientoCaja Direccion { get; set; }

        public decimal Importe { get; set; }

        public string UsuarioNombre { get; set; } = null!;

        public string? MedioPagoNombre { get; set; }

        public string? CategoriaGastoNombre { get; set; }

        public string? Concepto { get; set; }

        public string? Observaciones { get; set; }

        public int? TurnoCajaId { get; set; }

        public int? MovimientoOrigenId { get; set; }

        public bool EsReversion { get; set; }
    }
}