using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel.Autenticacion
{
    public class RecuperarPasswordVM
    {
        [Required(
            ErrorMessage =
                "Debe ingresar su correo electrónico.")]
        [EmailAddress(
            ErrorMessage =
                "Ingrese un correo electrónico válido.")]
        [StringLength(
            100,
            ErrorMessage =
                "El correo electrónico no puede superar los 100 caracteres.")]
        public string Email { get; set; } =
            string.Empty;
    }
}
