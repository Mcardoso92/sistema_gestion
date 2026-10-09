using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.InventarioFisico
{
    public class InventarioFisicoCrearVM
    {
        public int EmpresaId { get; set; }
        public int? CategoriaId { get; set; }
        public string? Busqueda { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalProductos { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
        public string Motivo { get; set; } = string.Empty;

        public List<int> ProductosIds { get; set; } = new();
        public List<InventarioFisicoProductoSeleccionVM> Productos { get; set; } = new();
    }

    public class InventarioFisicoProductoSeleccionVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoBarra { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
