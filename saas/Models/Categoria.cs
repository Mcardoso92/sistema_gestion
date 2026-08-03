using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; }
        public int EmpresaId { get; set; }
        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;
        [ValidateNever]
        public ICollection<Producto>? Productos { get; set; } = new List<Producto>();
    }
}
