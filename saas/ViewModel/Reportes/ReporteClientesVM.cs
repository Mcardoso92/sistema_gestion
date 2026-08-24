using Microsoft.AspNetCore.Mvc.Rendering;

namespace saas.ViewModel.Reportes
{
    public class ReporteClientesVM
    {
        public int? EmpresaId { get; set; }

        public string Estado { get; set; } = "activos";

        public string Actividad { get; set; } = "todos";

        public string? Busqueda { get; set; }

        public int CantidadClientes { get; set; }

        public int ClientesActivos { get; set; }

        public int ClientesInactivos { get; set; }

        public int ClientesConCompras { get; set; }

        public decimal ImporteTotalComprado { get; set; }

        public List<ReporteClienteFilaVM> Clientes { get; set; } =
            new List<ReporteClienteFilaVM>();

        public List<SelectListItem> Empresas { get; set; } =
            new List<SelectListItem>();
    }

    public class ReporteClienteFilaVM
    {
        public int ClienteId { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;

        public string? Documento { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public string Empresa { get; set; } = string.Empty;

        public int CantidadCompras { get; set; }

        public decimal ImporteComprado { get; set; }

        public DateTime? UltimaCompra { get; set; }

        public bool Estado { get; set; }
    }
}