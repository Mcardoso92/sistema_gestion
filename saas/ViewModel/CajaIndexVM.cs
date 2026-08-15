using Microsoft.AspNetCore.Mvc.Rendering;
using saas.Models.Enums;

namespace saas.ViewModel
{
    public class CajaIndexVM
    {
        public string? Busqueda { get; set; }

        public string? Estado { get; set; }

        public TipoCaja? Tipo { get; set; }

        public int? EmpresaId { get; set; }

        public List<SelectListItem> Empresas { get; set; }
            = new List<SelectListItem>();

        public List<CajaIndexItemVM> Cajas { get; set; }
            = new List<CajaIndexItemVM>();
    }
}