using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<Producto>? Productos { get; set; }
    }
}
