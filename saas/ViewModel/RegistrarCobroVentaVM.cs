using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class RegistrarCobroVentaVM
    {
        public int VentaId { get; set; }

        public decimal SaldoPendiente { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un medio de pago.")]
        public int MedioPagoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una caja.")]
        public int CajaId { get; set; }

        public List<MedioPagoOpcionSimpleVM> MediosPagoDisponibles { get; set; }
            = new List<MedioPagoOpcionSimpleVM>();

        public List<CajaOpcionSimpleVM> CajasDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();
    }
}