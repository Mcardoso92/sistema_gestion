using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace saas.Models
{
    // Conserva la tabla histórica original para que la ampliación no descarte
    // los cambios de costo que ya existen en las instalaciones.
    [Table("CambiosCostoProducto")]
    public class CambioValorProducto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int EmpresaId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public int? CompraId { get; set; }
        public int? CambioRevertidoId { get; set; }
        public Guid? OperacionId { get; set; }

        [Column("CostoAnterior")]
        public decimal ValorAnterior { get; set; }

        [Column("CostoNuevo")]
        public decimal ValorNuevo { get; set; }

        public TipoValorProducto TipoValor { get; set; } = TipoValorProducto.Costo;
        public DateTime Fecha { get; set; }
        public OrigenCambioValorProducto Origen { get; set; }

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

        [ValidateNever]
        public CambioValorProducto? CambioRevertido { get; set; }
    }
}
