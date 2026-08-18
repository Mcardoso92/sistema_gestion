using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaCreateVM
    {
        public int? ClienteId { get; set; }

        [ValidateNever]
        public string ClienteNombre { get; set; }
            = "Cliente ocasional";

        [MinLength(
            1,
            ErrorMessage = "Debe agregar al menos un producto a la venta.")]
        public List<VentaDetalleCreateVM> Detalles { get; set; }
            = new List<VentaDetalleCreateVM>();

        public List<VentaPagoCreateVM> Pagos { get; set; }
            = new List<VentaPagoCreateVM>();

        [ValidateNever]
        public List<MedioPagoOpcionSimpleVM> MediosPagoDisponibles { get; set; }
            = new List<MedioPagoOpcionSimpleVM>();

        [ValidateNever]
        public List<CajaOpcionSimpleVM> CajasDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();

        public int TotalLineas =>
            Detalles.Count;

        public int TotalUnidades =>
            Detalles.Sum(d => d.Cantidad);

        public decimal Total =>
            Detalles.Sum(d => d.Subtotal);

        public decimal TotalPagado =>
            Pagos.Sum(p => p.Importe);

        public decimal SaldoPendiente =>
            Math.Max(
                0,
                Total - TotalPagado);
    }
}