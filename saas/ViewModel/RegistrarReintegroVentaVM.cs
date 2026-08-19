using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class RegistrarReintegroVentaVM
    {
        public int VentaId { get; set; }

        public decimal ImporteDisponible { get; set; }

        public decimal Importe =>
            Detalles
                .Sum(d =>
                    d.Subtotal);

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar un medio de pago.")]
        public int MedioPagoId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar una caja.")]
        public int CajaId { get; set; }

        public List<ReintegroVentaDetalleVM> Detalles { get; set; }
            = new List<ReintegroVentaDetalleVM>();

        public List<MedioPagoOpcionSimpleVM> MediosPagoDisponibles { get; set; }
            = new List<MedioPagoOpcionSimpleVM>();

        public List<CajaOpcionSimpleVM> CajasDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();
    }
}