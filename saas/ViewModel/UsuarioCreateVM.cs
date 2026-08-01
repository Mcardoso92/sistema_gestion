using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class UsuarioCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar una empresa.")]
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public string Rol { get; set; } = null!;
        public bool Estado { get; set; } = true;

        public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> Empresas { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
