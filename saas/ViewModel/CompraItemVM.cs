namespace saas.ViewModel
{
    public class CompraItemVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string ProveedorNombre { get; set; } = string.Empty;

        public string? TipoComprobante { get; set; }

        public string? NumeroComprobante { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public string EmpresaNombre { get; set; } = string.Empty;
    }
}