namespace saas.ViewModel
{
    public class VentaIndexItemVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string ClienteNombre { get; set; } = "Consumidor Final";

        public string UsuarioNombre { get; set; } = null!;

        public string EmpresaNombre { get; set; } = null!;

        public decimal Total { get; set; }

        public decimal TotalCobrado { get; set; }

        public decimal SaldoPendiente =>
            Math.Max(
                0,
                Total - TotalCobrado);

        public bool EstaCobrada =>
            SaldoPendiente <= 0;

        public bool TienePagoParcial =>
            TotalCobrado > 0 &&
            SaldoPendiente > 0;

        public bool EstaPendienteDeCobro =>
            TotalCobrado <= 0 &&
            SaldoPendiente > 0;

        public bool Estado { get; set; }

        public int TotalUnidades { get; set; }
    }
}
