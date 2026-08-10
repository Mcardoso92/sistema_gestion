using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class StockAjusteVM
    {
        public int ProductoId { get; set; }

        [ValidateNever]
        public string ProductoNombre { get; set; } = null!;

        [ValidateNever]
        public string? CodigoBarra { get; set; }

        [ValidateNever]
        public int StockActual { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de ajuste.")]
        public TipoAjusteStockVM Tipo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
        public string Motivo { get; set; } = null!;
    }
}