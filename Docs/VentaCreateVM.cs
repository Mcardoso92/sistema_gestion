using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaCreateVM
    {
        public int? ClienteId { get; set; }

        public string ClienteNombre { get; set; } = "Cliente ocasional";

        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto a la venta.")]
        public List<VentaDetalleCreateVM> Detalles { get; set; } = new List<VentaDetalleCreateVM>();

        public int TotalLineas => Detalles.Count;

        public int TotalUnidades => Detalles.Sum(d => d.Cantidad);

        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}