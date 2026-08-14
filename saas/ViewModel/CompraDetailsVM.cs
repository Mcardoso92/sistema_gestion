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
    }
}