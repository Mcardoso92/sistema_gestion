using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class CambioCostoProducto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int EmpresaId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public int? CompraId { get; set; }
        public decimal CostoAnterior { get; set; }
        public decimal CostoNuevo { get; set; }
        public DateTime Fecha { get; set; }
        public OrigenCambioCostoProducto Origen { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        [ValidateNever]
        public Producto Producto { get; set; } = null!;

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public Compra? Compra { get; set; }
    }
}
