using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class DevolucionCompra
    {
        public int Id { get; set; }

        public int CompraId { get; set; }

        public int EmpresaId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        [StringLength(
            500,
            ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionId { get; set; }

        [StringLength(
            500,
            ErrorMessage = "El motivo de anulación no puede superar los 500 caracteres.")]
        public string? MotivoAnulacion { get; set; }

        [ValidateNever]
        public Compra Compra { get; set; } = null!;

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public Usuario? UsuarioAnulacion { get; set; }

        [ValidateNever]
        public ICollection<DetalleDevolucionCompra> Detalles { get; set; }
            = new List<DetalleDevolucionCompra>();

        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; }
            = new List<MovimientoStock>();
    }
}