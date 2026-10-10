namespace saas.ViewModel.CuentaCorriente
{
    public class CuentaCorrienteVM
    {
        public int TitularId { get; set; }
        public string Titular { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public bool EsCliente { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal SaldoARecuperar { get; set; }
        public string EstadoFiltro { get; set; } = "todos";
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public List<CuentaCorrienteMovimientoVM> Movimientos { get; set; } = new();
    }

    public class CuentaCorrienteMovimientoVM
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public decimal EfectoSaldo { get; set; }
        public decimal SaldoResultante { get; set; }
        public bool Activo { get; set; }
        public string ControladorReferencia { get; set; } = string.Empty;
        public int IdReferencia { get; set; }

        // Permite ordenar operaciones con la misma fecha de forma estable:
        // primero el comprobante y después sus cobros, pagos o ajustes.
        public int Orden { get; set; }
        public int IdOperacion { get; set; }
    }
}
