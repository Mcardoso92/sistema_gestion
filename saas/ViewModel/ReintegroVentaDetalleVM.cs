using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class ReintegroVentaDetalleVM
    {
        public int ProductoId { get; set; }

        [ValidateNever]
        public string ProductoNombre { get; set; } = null!;

        [ValidateNever]
        public int CantidadVendida { get; set; }

        [ValidateNever]
        public int CantidadYaReintegrada { get; set; }

        public int CantidadDisponible =>
            Math.Max(
                0,
                CantidadVendida - CantidadYaReintegrada);

        [ValidateNever]
        public decimal PrecioUnitario { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "La cantidad a reintegrar no puede ser negativa.")]
        public int CantidadReintegrar { get; set; }

        public decimal Subtotal =>
            PrecioUnitario * CantidadReintegrar;
    }
}