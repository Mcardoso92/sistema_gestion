namespace saas.ViewModel
{
    public class RegularizacionTurnoResumenVM
    {
        public bool Regularizado { get; set; }

        public int? MovimientoCajaId { get; set; }

        public DateTime? FechaRegularizacion { get; set; }

        public string? UsuarioRegularizacionNombre { get; set; }

        public decimal? Importe { get; set; }

        public string? Motivo { get; set; }
    }
}