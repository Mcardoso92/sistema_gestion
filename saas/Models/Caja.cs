using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Caja
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        public TipoCaja Tipo { get; set; }

        public bool PermiteTurnos { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El fondo fijo no puede ser negativo.")]
        public decimal FondoFijo { get; set; }

        public bool Estado { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }

        public int EmpresaId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public ICollection<CajaMedioPago> CajaMediosPago { get; set; } = new List<CajaMedioPago>();

        [ValidateNever]
        public ICollection<TurnoCaja> TurnosCaja { get; set; } = new List<TurnoCaja>();

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();

        [ValidateNever]
        public ICollection<TransferenciaCaja> TransferenciasOrigen { get; set; } = new List<TransferenciaCaja>();

        [ValidateNever]
        public ICollection<TransferenciaCaja> TransferenciasDestino { get; set; } = new List<TransferenciaCaja>();
    }
}