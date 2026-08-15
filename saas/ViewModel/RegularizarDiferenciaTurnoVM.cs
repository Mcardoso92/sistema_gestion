using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class RegularizarDiferenciaTurnoVM
    {
        public int TurnoCajaId { get; set; }

        public string CajaNombre { get; set; } = null!;

        public string UsuarioTurnoNombre { get; set; } = null!;

        public DateTime FechaCierre { get; set; }

        public decimal EfectivoEsperado { get; set; }

        public decimal EfectivoContado { get; set; }

        public decimal Diferencia { get; set; }

        public bool EsSobrante => Diferencia > 0;

        public bool EsFaltante => Diferencia < 0;

        public decimal ImporteARegularizar => Math.Abs(Diferencia);

        [Required(ErrorMessage = "El motivo de regularización es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = null!;
    }
}