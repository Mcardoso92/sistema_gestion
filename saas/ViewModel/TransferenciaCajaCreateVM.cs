using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class TransferenciaCajaCreateVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una caja de origen.")]
        public int CajaOrigenId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una caja de destino.")]
        public int CajaDestinoId { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
        public string Motivo { get; set; } = null!;

        public decimal SaldoDisponibleOrigen { get; set; }

        public List<CajaOpcionSimpleVM> CajasOrigenDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();

        public List<CajaOpcionSimpleVM> CajasDestinoDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();
    }
}