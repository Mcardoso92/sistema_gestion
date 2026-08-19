using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DetalleReintegroVenta
    {
        public int Id { get; set; }

        public int ReintegroVentaId { get; set; }

        public int ProductoId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "El precio unitario debe ser mayor a 0.")]
        public decimal PrecioUnitario { get; set; }

        [Range(
            0.01,
            999999999.99,
            ErrorMessage = "El subtotal debe ser mayor a 0.")]
        public decimal Subtotal { get; set; }

        [ValidateNever]
        public ReintegroVenta ReintegroVenta { get; set; } = null!;

        [ValidateNever]
        public Producto Producto { get; set; } = null!;
    }
}