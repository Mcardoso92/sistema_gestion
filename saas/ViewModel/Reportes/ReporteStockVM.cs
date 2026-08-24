using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel.Reportes
{
    public class ReporteStockVM
    {
        public int? CategoriaId { get; set; }

        public int? EmpresaId { get; set; }

        public string Situacion { get; set; } = "todos";

        public int CantidadProductos { get; set; }

        public int UnidadesStock { get; set; }

        public int ProductosStockBajo { get; set; }

        public decimal ValorInventarioCosto { get; set; }

        public decimal ValorInventarioVenta { get; set; }

        public List<ReporteStockFilaVM> Productos { get; set; } =
            new List<ReporteStockFilaVM>();

        public List<SelectListItem> Categorias { get; set; } =
            new List<SelectListItem>();

        public List<SelectListItem> Empresas { get; set; } =
            new List<SelectListItem>();
    }

    public class ReporteStockFilaVM
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? CodigoBarra { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Empresa { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int PuntoReposicion { get; set; }

        public decimal PrecioCosto { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal ValorCosto { get; set; }

        public decimal ValorVenta { get; set; }

        public string Situacion { get; set; } = string.Empty;
    }
}