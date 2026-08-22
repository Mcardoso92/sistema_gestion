using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DetalleDevolucionCompra
    {
        public int Id { get; set; }

        public int DevolucionCompraId { get; set; }

        public int DetalleCompraId { get; set; }

        public int ProductoId { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(
            0,
            double.MaxValue,
            ErrorMessage = "El precio unitario no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        [ValidateNever]
        public DevolucionCompra DevolucionCompra { get; set; } = null!;

        [ValidateNever]
        public DetalleCompra DetalleCompra { get; set; } = null!;

        [ValidateNever]
        public Producto Producto { get; set; } = null!;
    }
}