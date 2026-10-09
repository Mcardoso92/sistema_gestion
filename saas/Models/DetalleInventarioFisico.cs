using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DetalleInventarioFisico
    {
        public int Id { get; set; }
        public int InventarioFisicoId { get; set; }
        public int ProductoId { get; set; }

        [Range(0, int.MaxValue)]
        public int StockTeorico { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad contada no puede ser negativa.")]
        public int? StockContado { get; set; }

        [ValidateNever]
        public InventarioFisico InventarioFisico { get; set; } = null!;

        [ValidateNever]
        public Producto Producto { get; set; } = null!;
    }
}
