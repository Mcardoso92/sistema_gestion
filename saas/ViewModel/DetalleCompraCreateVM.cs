using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class DetalleCompraCreateVM
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de costo no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        public decimal PrecioVentaActual { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El nuevo precio de venta no puede ser negativo.")]
        public decimal? NuevoPrecioVenta { get; set; }
    }
}