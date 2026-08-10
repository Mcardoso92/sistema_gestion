using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Venta
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public bool Estado { get; set; }
        public int EmpresaId { get; set; }
        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;
        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;
        public int? ClienteId { get; set; }
        [ValidateNever]
        public Cliente? Cliente { get; set; }
        [ValidateNever]
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();

    }
}
