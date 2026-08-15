using saas.Models.Enums;

namespace saas.ViewModel
{
    public class TurnoCajaDetailsVM
    {
        public int Id { get; set; }

        public string EmpresaNombre { get; set; } = null!;

        public string CajaNombre { get; set; } = null!;

        public string UsuarioAperturaNombre { get; set; } = null!;

        public DateTime FechaApertura { get; set; }

        public EstadoTurnoCaja Estado { get; set; }

        public decimal FondoFijoAplicado { get; set; }

        public DateTime? FechaCierre { get; set; }

        public string? UsuarioCierreNombre { get; set; }

        public bool CierreForzado { get; set; }

        public string? MotivoCierreForzado { get; set; }

        public decimal? EfectivoEsperado { get; set; }

        public decimal? EfectivoContado { get; set; }

        public decimal? Diferencia { get; set; }

        public decimal? ImporteRendido { get; set; }

        public List<TurnoMovimientoResumenVM> Movimientos { get; set; }
            = new List<TurnoMovimientoResumenVM>();
            
        public RegularizacionTurnoResumenVM Regularizacion { get; set; }
            = new RegularizacionTurnoResumenVM();
    }
}