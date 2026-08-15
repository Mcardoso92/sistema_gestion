using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class AperturaTurnoVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una caja.")]
        public int CajaId { get; set; }

        public string? CajaNombre { get; set; }

        public decimal FondoFijo { get; set; }

        public List<CajaTurnoOpcionVM> CajasDisponibles { get; set; }
            = new List<CajaTurnoOpcionVM>();
    }
}