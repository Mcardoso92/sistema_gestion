using saas.Models.Enums;

namespace saas.ViewModel
{
    public class MedioPagoIndexVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }
        public TipoMedioPago Tipo { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaAlta { get; set; }

        public string EmpresaNombre { get; set; } = null!;
    }
}