using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        [ValidateNever]
        public Venta Venta { get; set; } = null!;
        public int ProductoId { get; set; }
        [ValidateNever]
        public Producto Producto { get; set; } = null!;
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "El precio unitario debe ser mayor a cero.")]
        public decimal PrecioUnitario { get; set; }
        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "El subtotal debe ser mayor a cero.")]
        public decimal Subtotal { get; set; }

    }
}
