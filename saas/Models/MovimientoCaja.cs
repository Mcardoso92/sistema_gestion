using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class MovimientoCaja
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        public int CajaId { get; set; }

        public TipoMovimientoCaja Tipo { get; set; }

        public DireccionMovimientoCaja Direccion { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        public DateTime Fecha { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public int? MedioPagoId { get; set; }

        public int? TurnoCajaId { get; set; }

        public int? CategoriaGastoId { get; set; }

        [StringLength(250, ErrorMessage = "El concepto no puede superar los 250 caracteres.")]
        public string? Concepto { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        // Reversión
        public int? MovimientoOrigenId { get; set; }

        // Origen financiero
        public int? CobroVentaId { get; set; }

        public int? PagoProveedorId { get; set; }

        public int? ReintegroVentaId { get; set; }

        public int? ReintegroProveedorId { get; set; }

        public int? TransferenciaCajaId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Caja Caja { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public MedioPago? MedioPago { get; set; }

        [ValidateNever]
        public TurnoCaja? TurnoCaja { get; set; }

        [ValidateNever]
        public CategoriaGasto? CategoriaGasto { get; set; }

        [ValidateNever]
        public MovimientoCaja? MovimientoOrigen { get; set; }

        [ValidateNever]
        public ICollection<MovimientoCaja> Reversiones { get; set; }
            = new List<MovimientoCaja>();

        [ValidateNever]
        public CobroVenta? CobroVenta { get; set; }

        [ValidateNever]
        public PagoProveedor? PagoProveedor { get; set; }

        [ValidateNever]
        public ReintegroVenta? ReintegroVenta { get; set; }

        [ValidateNever]
        public ReintegroProveedor? ReintegroProveedor { get; set; }

        [ValidateNever]
        public TransferenciaCaja? TransferenciaCaja { get; set; }
    }
}