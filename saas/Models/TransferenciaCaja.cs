using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class TransferenciaCaja
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        public int CajaOrigenId { get; set; }

        public int CajaDestinoId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public int? TurnoCajaId { get; set; }

        public DateTime Fecha { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor a 0.")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(250, ErrorMessage = "El motivo no puede superar los 250 caracteres.")]
        public string Motivo { get; set; } = null!;

        public EstadoTransferenciaCaja Estado { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionId { get; set; }

        [StringLength(500, ErrorMessage = "El motivo de anulación no puede superar los 500 caracteres.")]
        public string? MotivoAnulacion { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Caja CajaOrigen { get; set; } = null!;

        [ValidateNever]
        public Caja CajaDestino { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public TurnoCaja? TurnoCaja { get; set; }

        [ValidateNever]
        public Usuario? UsuarioAnulacion { get; set; }

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
            = new List<MovimientoCaja>();
    }
}