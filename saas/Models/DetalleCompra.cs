using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DetalleCompra
    {
        public int Id { get; set; }

        public int CompraId { get; set; }

        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
        public decimal PrecioCostoAnterior { get; set; }

        public decimal? PrecioVentaAnterior { get; set; }

        public decimal? PrecioVentaNuevo { get; set; }

        [ValidateNever]
        public Compra Compra { get; set; } = null!;

        [ValidateNever]
        public Producto Producto { get; set; } = null!;
    }
}