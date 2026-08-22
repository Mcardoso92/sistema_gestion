using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class AnularPagoProveedorVM
    {
        public int PagoProveedorId { get; set; }

        [ValidateNever]
        public int CompraId { get; set; }

        [ValidateNever]
        public decimal Importe { get; set; }

        [ValidateNever]
        public string MedioPagoNombre { get; set; } = null!;

        [Required(ErrorMessage = "Debe indicar el motivo de la anulación.")]
        [StringLength(
            500,
            ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}