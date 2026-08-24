using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel.Reportes
{
    public class ReporteVentasVM
    {
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; }

        public int? ClienteId { get; set; }

        public int? EmpresaId { get; set; }

        public decimal TotalVendido { get; set; }

        public int CantidadVentas { get; set; }

        public decimal TicketPromedio { get; set; }

        public List<ReporteVentaFilaVM> Ventas { get; set; } =
            new List<ReporteVentaFilaVM>();

        public List<SelectListItem> Clientes { get; set; } =
            new List<SelectListItem>();

        public List<SelectListItem> Empresas { get; set; } =
            new List<SelectListItem>();
    }

    public class ReporteVentaFilaVM
    {
        public int VentaId { get; set; }

        public DateTime Fecha { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }

        public decimal Total { get; set; }
    }
}