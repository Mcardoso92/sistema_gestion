namespace saas.ViewModel
{
    public class TurnoCobroMedioPagoResumenVM
    {
        public int MedioPagoId { get; set; }

        public string MedioPagoNombre { get; set; } = null!;

        public decimal Total { get; set; }

        public int CantidadCobros { get; set; }
    }
}