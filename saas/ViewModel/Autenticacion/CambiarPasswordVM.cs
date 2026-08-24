using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.Autenticacion
{
    public class CambiarPasswordVM
    {
        [Required(
            ErrorMessage =
                "Debe ingresar su contraseña actual.")]
        [DataType(DataType.Password)]
        public string PasswordActual { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage =
                "Debe ingresar una nueva contraseña.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "La contraseña debe tener al menos 6 caracteres.")]
        public string PasswordNueva { get; set; } =
            string.Empty;

        [Required(
            ErrorMessage =
                "Debe confirmar la nueva contraseña.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(PasswordNueva),
            ErrorMessage =
                "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } =
            string.Empty;
    }
}