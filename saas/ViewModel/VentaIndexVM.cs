using Microsoft.AspNetCore.Mvc.Rendering;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class VentaIndexVM
    {
        public string? Buscar { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }

        public int? EmpresaId { get; set; }

        public string? UsuarioId { get; set; }

        public bool? Estado { get; set; }
        public EstadoCobroVentaFiltro? EstadoCobro { get; set; }

        public List<SelectListItem> Empresas { get; set; } = new();

        public List<SelectListItem> Usuarios { get; set; } = new();

        public int CantidadConSaldoPendiente { get; set; }

        public decimal TotalPendienteCobro { get; set; }

        public List<VentaIndexItemVM> Ventas { get; set; } = new();
    }
}