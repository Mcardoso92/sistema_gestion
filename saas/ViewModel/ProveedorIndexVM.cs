using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel
{
    public class ProveedorIndexVM
    {
        public string? Busqueda { get; set; }
        public string Estado { get; set; } = "activos";
        public int? EmpresaId { get; set; }

        public List<SelectListItem> Empresas { get; set; } = new();
        public List<ProveedorIndexItemVM> Proveedores { get; set; } = new();
    }
}