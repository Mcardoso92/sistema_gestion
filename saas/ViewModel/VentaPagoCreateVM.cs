using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaPagoCreateVM
    {
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

        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        public string? MedioPagoNombre { get; set; }

        public string? CajaNombre { get; set; }
    }
}