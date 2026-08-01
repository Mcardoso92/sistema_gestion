using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class UsuarioEditVM
    {
        public string Id { get; set; } = null!;

        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Apellido { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public bool Estado { get; set; }

        [Required]
        public string Rol { get; set; } = null!;

        public int EmpresaId { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Empresas { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
