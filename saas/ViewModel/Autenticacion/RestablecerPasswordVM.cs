using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.Autenticacion
{
    public class RestablecerPasswordVM
    {
        [Required]
        public string Email { get; set; } =
            string.Empty;

        [Required]
        public string Token { get; set; } =
            string.Empty;

        [Required(ErrorMessage = "Debe ingresar una nueva contraseña.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } =
            string.Empty;
    }
}