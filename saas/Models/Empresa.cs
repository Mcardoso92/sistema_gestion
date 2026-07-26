using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public ICollection<Usuario>? Usuarios { get; set; }
        public ICollection<Producto>? Productos { get; set; }
        public ICollection<Venta>? Ventas { get; set; }
        public ICollection<Categoria>? Categorias { get; set; }

    }
}
