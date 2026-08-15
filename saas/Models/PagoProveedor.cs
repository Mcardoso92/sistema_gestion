using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class PagoProveedor
    {
        public int Id { get; set; }

        public int CompraId { get; set; }

        public int EmpresaId { get; set; }

        public int CajaId { get; set; }

        public int MedioPagoId { get; set; }

        public int? TurnoCajaId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public DateTime Fecha { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        public EstadoPago Estado { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionId { get; set; }

        [StringLength(500, ErrorMessage = "El motivo de anulación no puede superar los 500 caracteres.")]
        public string? MotivoAnulacion { get; set; }

        [ValidateNever]
        public Compra Compra { get; set; } = null!;

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Caja Caja { get; set; } = null!;

        [ValidateNever]
        public MedioPago MedioPago { get; set; } = null!;

        [ValidateNever]
        public TurnoCaja? TurnoCaja { get; set; }

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public MovimientoCaja? MovimientoCaja { get; set; }

        [ValidateNever]
        public Usuario? UsuarioAnulacion { get; set; }
    }
}