using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class CierreTurnoVM
    {
        public int TurnoCajaId { get; set; }

        public string CajaNombre { get; set; } = null!;

        public string UsuarioAperturaNombre { get; set; } = null!;

        public DateTime FechaApertura { get; set; }

        public decimal FondoFijoAplicado { get; set; }

        public decimal EfectivoEsperado { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El efectivo contado no puede ser negativo.")]
        public decimal EfectivoContado { get; set; }

        public decimal Diferencia { get; set; }

        public decimal ImporteRendirSugerido { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El importe rendido no puede ser negativo.")]
        public decimal ImporteRendido { get; set; }

        public bool CierreForzado { get; set; }

        [StringLength(500, ErrorMessage = "El motivo del cierre forzado no puede superar los 500 caracteres.")]
        public string? MotivoCierreForzado { get; set; }
    }
}