using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = null!;
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string Apellido { get; set; } = null!;
        [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres.")]
        public string? ImagenPerfil { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = null!;
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        [ValidateNever]
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();

    }
}
