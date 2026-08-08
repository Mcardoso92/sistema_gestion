using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaDetalleCreateVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "El producto seleccionado no es válido.")]
        public int ProductoId { get; set; }

        [ValidateNever]
        public string ProductoNombre { get; set; } = null!;

        [ValidateNever]
        public string? CodigoBarra { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [ValidateNever]
        public decimal PrecioUnitario { get; set; }

        [ValidateNever]
        public int StockDisponible { get; set; }

        [ValidateNever]
        public decimal Subtotal { get; set; }

        [ValidateNever]
        public bool StockSuficiente { get; set; }
    }
}
