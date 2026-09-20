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

        // El listado recibe los importes ya calculados desde la consulta para no repetir
        // reglas financieras en la vista.
        public decimal TotalPagado { get; set; }

        public decimal TotalDevuelto { get; set; }

        public decimal TotalNeto =>
            Math.Max(0, Total - TotalDevuelto);

        public decimal SaldoPendiente =>
            Math.Max(0, TotalNeto - TotalPagado);

        public bool EstaPagada =>
            SaldoPendiente <= 0;

        public bool TienePagoParcial =>
            TotalPagado > 0 &&
            SaldoPendiente > 0;

        public bool Estado { get; set; }

        public string EmpresaNombre { get; set; } = string.Empty;
    }
}
