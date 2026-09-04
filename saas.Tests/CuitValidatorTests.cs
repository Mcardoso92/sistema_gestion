using saas.Services;

namespace saas.Tests;

public class CuitValidatorTests
{
    [Theory]
    [InlineData("20123456786", "20-12345678-6")]
    [InlineData("20-12345678-6", "20-12345678-6")]
    public void Formatear_CuitDeOnceDigitosDevuelveFormatoLegible(string cuit, string esperado)
    {
        Assert.Equal(esperado, CuitValidator.Formatear(cuit));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    public void Formatear_ValorAusenteOIncompletoNoInventaUnCuit(string? cuit)
    {
        Assert.Equal(cuit, CuitValidator.Formatear(cuit));
    }
}
