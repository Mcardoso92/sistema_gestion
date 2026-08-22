using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.ReintegroProveedor
{
    public class AnularReintegroProveedorVM
    {
        public int ReintegroProveedorId { get; set; }

        [ValidateNever]
        public int CompraId { get; set; }

        [ValidateNever]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "Debe indicar el motivo de la anulación.")]
        [StringLength(
            500,
            ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}