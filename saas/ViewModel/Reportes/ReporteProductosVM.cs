using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel.Reportes
{
    public class ReporteProductosVM
    {
        public int? CategoriaId { get; set; }

        public int? EmpresaId { get; set; }

        public string Estado { get; set; } = "activos";

        public string? Busqueda { get; set; }

        public int CantidadProductos { get; set; }

        public int ProductosActivos { get; set; }

        public int ProductosInactivos { get; set; }

        public decimal MargenPromedioPorcentaje { get; set; }

        public List<ReporteProductoFilaVM> Productos { get; set; } =
            new List<ReporteProductoFilaVM>();

        public List<SelectListItem> Categorias { get; set; } =
            new List<SelectListItem>();

        public List<SelectListItem> Empresas { get; set; } =
            new List<SelectListItem>();
    }

    public class ReporteProductoFilaVM
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? CodigoBarra { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Empresa { get; set; } = string.Empty;

        public decimal PrecioCosto { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal MargenImporte { get; set; }

        public decimal MargenPorcentaje { get; set; }

        public int Stock { get; set; }

        public bool Estado { get; set; }
    }
}