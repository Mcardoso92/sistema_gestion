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
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe incluir una mayúscula, una minúscula y un número.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } =
            string.Empty;
    }
}