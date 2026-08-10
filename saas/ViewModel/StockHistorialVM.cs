using Microsoft.AspNetCore.Mvc.Rendering;
using saas.Models.Enums;

namespace saas.ViewModel
{
    public class StockHistorialVM
    {
        public int? ProductoId { get; set; }
        public TipoMovimientoStock? Tipo { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? EmpresaId { get; set; }

        public string? ProductoNombre { get; set; }
        public string? CodigoBarra { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? EmpresaNombre { get; set; }
        public int? StockActual { get; set; }
        public int? PuntoReposicion { get; set; }
        public bool? ProductoActivo { get; set; }

        public List<SelectListItem> Productos { get; set; } = new();
        public List<SelectListItem> Empresas { get; set; } = new();
        public List<StockHistorialItemVM> Movimientos { get; set; } = new();
    }
}