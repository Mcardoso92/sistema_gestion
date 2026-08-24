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
        public string Email { get; set; } =
            string.Empty;
    }
}