using saas.Models.Enums;

namespace saas.ViewModel
{
    public class TurnoMovimientoResumenVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public TipoMovimientoCaja Tipo { get; set; }

        public DireccionMovimientoCaja Direccion { get; set; }

        public string CajaNombre { get; set; } = null!;

        public string? MedioPagoNombre { get; set; }

        public string? Concepto { get; set; }

        public decimal Importe { get; set; }
    }
}