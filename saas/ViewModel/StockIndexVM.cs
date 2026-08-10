using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel
{
    public class StockIndexVM
    {
        public string? Busqueda { get; set; }

        public string EstadoStock { get; set; } = "todos";

        public int? EmpresaId { get; set; }

        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();

        public List<StockIndexItemVM> Productos { get; set; } = new List<StockIndexItemVM>();
    }
}