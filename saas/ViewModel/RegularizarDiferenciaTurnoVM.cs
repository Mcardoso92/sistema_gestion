using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class RegularizarDiferenciaTurnoVM
    {
        public int TurnoCajaId { get; set; }

        [ValidateNever]
        public string CajaNombre { get; set; } = null!;

        [ValidateNever]
        public string UsuarioTurnoNombre { get; set; } = null!;

        [ValidateNever]
        public DateTime FechaCierre { get; set; }

        [ValidateNever]
        public decimal EfectivoEsperado { get; set; }

        [ValidateNever]
        public decimal EfectivoContado { get; set; }

        [ValidateNever]
        public decimal Diferencia { get; set; }

        public bool EsSobrante => Diferencia > 0;

        public bool EsFaltante => Diferencia < 0;

        public decimal ImporteARegularizar =>
            Math.Abs(Diferencia);

        [Required(ErrorMessage = "El motivo de regularización es obligatorio.")]
        [StringLength(
            500,
            ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = null!;
    }
}