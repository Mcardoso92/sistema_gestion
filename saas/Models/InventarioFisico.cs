using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class InventarioFisico
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }

        [Required]
        public string UsuarioInicioId { get; set; } = null!;

        public string? UsuarioConfirmacionId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public EstadoInventarioFisico Estado { get; set; } = EstadoInventarioFisico.Abierto;

        [Required]
        [StringLength(250)]
        public string Motivo { get; set; } = null!;

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Usuario UsuarioInicio { get; set; } = null!;

        [ValidateNever]
        public Usuario? UsuarioConfirmacion { get; set; }

        [ValidateNever]
        public ICollection<DetalleInventarioFisico> Detalles { get; set; } = new List<DetalleInventarioFisico>();
    }
}
