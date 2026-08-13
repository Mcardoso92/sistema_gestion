using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaDetalleCreateVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "El producto seleccionado no es válido.")]
        public int ProductoId { get; set; }

        public string ProductoNombre { get; set; } = null!;

        public string? CodigoBarra { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int StockDisponible { get; set; }

        public decimal Subtotal { get; set; }

        public bool StockSuficiente { get; set; }
    }
}