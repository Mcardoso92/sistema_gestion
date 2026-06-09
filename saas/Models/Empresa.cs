using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        public string? ImagenEmpresa { get; set; }
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public ICollection<Usuario>? Usuarios { get; set; }
        public ICollection<Producto>? Productos { get; set; }
        public ICollection<Venta>? Ventas { get; set; }

    }
}
