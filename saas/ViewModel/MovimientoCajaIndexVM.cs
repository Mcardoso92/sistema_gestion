using saas.Models.Enums;

namespace saas.ViewModel
{
    public class MovimientoCajaIndexVM
    {
        public List<MovimientoCajaResumenVM> Movimientos { get; set; }
            = new List<MovimientoCajaResumenVM>();

        // Filtros
        public int? CajaId { get; set; }

        public int? MedioPagoId { get; set; }

        public int? CategoriaGastoId { get; set; }

        public int? TurnoCajaId { get; set; }

        public string? UsuarioId { get; set; }

        public TipoMovimientoCaja? Tipo { get; set; }

        public DireccionMovimientoCaja? Direccion { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public int? EmpresaId { get; set; }

        // Opciones para filtros
        public List<CajaOpcionSimpleVM> CajasDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();

        public List<MedioPagoOpcionSimpleVM> MediosPagoDisponibles { get; set; }
            = new List<MedioPagoOpcionSimpleVM>();

        public List<CategoriaGastoOpcionVM> CategoriasDisponibles { get; set; }
            = new List<CategoriaGastoOpcionVM>();

        public List<UsuarioOpcionVM> UsuariosDisponibles { get; set; }
            = new List<UsuarioOpcionVM>();

        // Resumen
        public decimal TotalIngresos { get; set; }

        public decimal TotalEgresos { get; set; }

        public decimal NetoPeriodo { get; set; }
    }
}