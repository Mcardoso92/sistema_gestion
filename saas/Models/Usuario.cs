using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }
        public string? ImagenPerfil { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }

    }
}
