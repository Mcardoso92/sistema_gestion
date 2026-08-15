using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class TurnoCaja
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        public int CajaId { get; set; }

        [Required]
        public string UsuarioAperturaId { get; set; } = null!;

        public DateTime FechaApertura { get; set; }

        public EstadoTurnoCaja Estado { get; set; }

        public DateTime? FechaCierre { get; set; }

        public string? UsuarioCierreId { get; set; }

        public bool CierreForzado { get; set; }

        [StringLength(500, ErrorMessage = "El motivo del cierre forzado no puede superar los 500 caracteres.")]
        public string? MotivoCierreForzado { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El fondo fijo aplicado no puede ser negativo.")]
        public decimal FondoFijoAplicado { get; set; }

        public decimal? EfectivoEsperado { get; set; }

        public decimal? EfectivoContado { get; set; }

        public decimal? Diferencia { get; set; }

        public decimal? ImporteRendido { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Caja Caja { get; set; } = null!;

        [ValidateNever]
        public Usuario UsuarioApertura { get; set; } = null!;

        [ValidateNever]
        public Usuario? UsuarioCierre { get; set; }

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
            = new List<MovimientoCaja>();
    }
}