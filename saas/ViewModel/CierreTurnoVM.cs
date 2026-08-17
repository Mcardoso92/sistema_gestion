using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace saas.ViewModel
{
    public class CierreTurnoVM
    {
        public int TurnoCajaId { get; set; }
        [ValidateNever]
        public string CajaNombre { get; set; } = null!;
        [ValidateNever]
        public string UsuarioAperturaNombre { get; set; } = null!;
        [ValidateNever]
        public DateTime FechaApertura { get; set; }
        [ValidateNever]
        public decimal FondoFijoAplicado { get; set; }
        [ValidateNever]
        public decimal EfectivoEsperado { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El efectivo contado no puede ser negativo.")]
        public decimal EfectivoContado { get; set; }
        [ValidateNever]
        public decimal Diferencia { get; set; }
        [ValidateNever]
        public decimal ImporteRendirSugerido { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El importe rendido no puede ser negativo.")]
        public decimal ImporteRendido { get; set; }

        public bool CierreForzado { get; set; }

        [StringLength(500, ErrorMessage = "El motivo del cierre forzado no puede superar los 500 caracteres.")]
        public string? MotivoCierreForzado { get; set; }
        public int? CajaDestinoId { get; set; }
        [ValidateNever]
        public List<CajaTurnoOpcionVM> CajasDestinoDisponibles { get; set; }
            = new List<CajaTurnoOpcionVM>();
    }
}