using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class DetalleCompraCreateVM
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El costo unitario debe ser mayor a 0.")]
        public decimal PrecioUnitario { get; set; }

        public decimal PrecioVentaActual { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El nuevo precio de venta no puede ser negativo.")]
        public decimal? NuevoPrecioVenta { get; set; }

        public bool EsProductoNuevo { get; set; }

        [StringLength(100, ErrorMessage = "El nombre del producto no puede superar los 100 caracteres.")]
        public string? ProductoNuevoNombre { get; set; }

        [StringLength(100, ErrorMessage = "El código de barras no puede superar los 100 caracteres.")]
        public string? ProductoNuevoCodigoBarra { get; set; }

        public int? ProductoNuevoCategoriaId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El punto de reposición no puede ser negativo.")]
        public int ProductoNuevoPuntoReposicion { get; set; }
    }
}
