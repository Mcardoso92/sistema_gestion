using saas.ViewModel.DevolucionCompra;

namespace saas.ViewModel
{
    public class CompraDetailsVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string ProveedorNombre { get; set; } = string.Empty;

        public string? TipoComprobante { get; set; }

        public string? NumeroComprobante { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public string? Observaciones { get; set; }

        public string UsuarioEmail { get; set; } = string.Empty;

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionEmail { get; set; }

        public string EmpresaNombre { get; set; } = string.Empty;

        public List<DetalleCompraDetailsVM> Detalles { get; set; } = new();
        // Pagos
        public decimal TotalPagado { get; set; }

        public decimal SaldoPendiente { get; set; }

        public List<PagoProveedorResumenVM> Pagos { get; set; }
            = new List<PagoProveedorResumenVM>();

        public RegistrarPagoProveedorVM NuevoPago { get; set; }
            = new RegistrarPagoProveedorVM();


        // Reintegros
        public decimal TotalReintegrado { get; set; }

        public decimal PendienteRecuperar { get; set; }

        public List<ReintegroProveedorResumenVM> ReintegrosProveedor { get; set; }
            = new List<ReintegroProveedorResumenVM>();

        public RegistrarReintegroProveedorVM NuevoReintegroProveedor { get; set; }
            = new RegistrarReintegroProveedorVM();

        // Devoluciones de mercadería
        public List<DevolucionCompraResumenVM> DevolucionesCompra { get; set; }
            = new List<DevolucionCompraResumenVM>();
    }
}