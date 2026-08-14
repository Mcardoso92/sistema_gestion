using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel
{
    public class CompraIndexVM
    {
        public string? Busqueda { get; set; }

        public string Estado { get; set; } = "activas";

        public int? ProveedorId { get; set; }

        public int? EmpresaId { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public List<CompraItemVM> Compras { get; set; } = new();

        public List<SelectListItem> Proveedores { get; set; } = new();

        public List<SelectListItem> Empresas { get; set; } = new();
    }
}