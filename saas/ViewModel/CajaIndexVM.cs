using saas.Models.Enums;

namespace saas.ViewModel
{
    public class CajaIndexVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public TipoCaja Tipo { get; set; }

        public bool PermiteTurnos { get; set; }

        public decimal FondoFijo { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaAlta { get; set; }

        public string EmpresaNombre { get; set; } = null!;

        public decimal SaldoActual { get; set; }
    }
}