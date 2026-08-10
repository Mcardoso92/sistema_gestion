using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; }  = new List<MovimientoStock>();

    }
}
