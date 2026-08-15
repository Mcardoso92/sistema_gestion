using saas.Models.Enums;

namespace saas.ViewModel
{
    public class TurnoCajaIndexVM
    {
        public int Id { get; set; }

        public string CajaNombre { get; set; } = null!;

        public string UsuarioAperturaNombre { get; set; } = null!;

        public DateTime FechaApertura { get; set; }

        public DateTime? FechaCierre { get; set; }

        public EstadoTurnoCaja Estado { get; set; }

        public bool CierreForzado { get; set; }

        public decimal FondoFijoAplicado { get; set; }

        public decimal? EfectivoEsperado { get; set; }

        public decimal? EfectivoContado { get; set; }

        public decimal? Diferencia { get; set; }

        public string EmpresaNombre { get; set; } = null!;
    }
}