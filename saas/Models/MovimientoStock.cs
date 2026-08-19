using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class MovimientoStock
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        [ValidateNever]
        public Producto Producto { get; set; } = null!;
        public int EmpresaId { get; set; }
        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;
        public TipoMovimientoStock Tipo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
        [Range(0, int.MaxValue)]
        public int StockAnterior { get; set; }
        [Range(0, int.MaxValue)]
        public int StockPosterior { get; set; }
        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
        [Required]
        public string UsuarioId { get; set; } = null!;
        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;
        public int? VentaId { get; set; }
        [ValidateNever]
        public Venta? Venta { get; set; }
        public int? CompraId { get; set; }
        [ValidateNever]
        public Compra? Compra { get; set; }

        public int? ReintegroVentaId { get; set; }

        [ValidateNever]
        public ReintegroVenta? ReintegroVenta { get; set; }
    }
}