using saas.Models.Enums;

namespace saas.ViewModel
{
    public class ReintegroProveedorResumenVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Importe { get; set; }

        public string MedioPagoNombre { get; set; } = null!;

        public string CajaNombre { get; set; } = null!;

        public string UsuarioNombre { get; set; } = null!;

        public EstadoReintegro Estado { get; set; }

        public int? TurnoCajaId { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionNombre { get; set; }

        public string? MotivoAnulacion { get; set; }
    }
}