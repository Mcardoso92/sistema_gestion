using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Producto
    {
        public int Id { get; set; }
        [StringLength(100)]
        public string? CodigoBarra { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }
        [StringLength(500)]
        public string? Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        [Range(0, 999999999.99, ErrorMessage = "El precio de costo debe ser mayor o igual a 0.")]
        public decimal PrecioCosto { get; set; }
        [Range(0, 999999999.99, ErrorMessage = "El precio de venta debe ser mayor o igual a 0.")]
        public decimal PrecioVenta { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "El punto de reposición no puede ser negativo.")]
        public int PuntoReposicion { get; set; }
        public bool Estado { get; set; }
        [Url(ErrorMessage = "Ingrese una URL válida.")]
        public string? UrlImagen { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }
    }
}
