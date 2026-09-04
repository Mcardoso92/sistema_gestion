using System.ComponentModel.DataAnnotations;
using saas.ViewModel.Autenticacion;

namespace saas.Tests;

public class RecuperarPasswordValidacionTests
{
    [Fact]
    public void Email_MayorA100CaracteresEsInvalido()
    {
        var modelo = new RecuperarPasswordVM
        {
            Email = $"{new string('a', 90)}@ejemplo.com"
        };
        var errores = new List<ValidationResult>();

        Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true);

        Assert.Contains(
            errores,
            error => error.MemberNames.Contains(nameof(RecuperarPasswordVM.Email)));
    }
}
