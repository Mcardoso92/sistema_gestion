using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class EgresoManualVM
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una caja.")]
        public int CajaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría de gasto.")]
        public int CategoriaGastoId { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio.")]
        [StringLength(250, ErrorMessage = "El concepto no puede superar los 250 caracteres.")]
        public string Concepto { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public decimal SaldoDisponible { get; set; }

        public List<CajaOpcionSimpleVM> CajasDisponibles { get; set; }
            = new List<CajaOpcionSimpleVM>();

        public List<CategoriaGastoOpcionVM> CategoriasDisponibles { get; set; }
            = new List<CategoriaGastoOpcionVM>();
    }
}