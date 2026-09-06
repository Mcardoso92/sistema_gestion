using saas.Services;

namespace saas.Tests;

public class CuitValidatorTests
{
    [Theory]
    [InlineData("20-12345678-6", true)]
    [InlineData("20123456786", true)]
    [InlineData("12345678", false)]
    [InlineData("ABC-123", false)]
    public void TieneFormatoCuit_DistingueCuitDeOtrosDocumentos(string valor, bool esperado)
    {
        Assert.Equal(esperado, CuitValidator.TieneFormatoCuit(valor));
    }

    [Theory]
    [InlineData("20123456786", "20-12345678-6")]
    [InlineData("12345678", "12345678")]
    [InlineData("20123456785", "20123456785")]
    public void FormatearSiEsCuit_NoModificaOtrosDocumentos(string valor, string esperado)
    {
        Assert.Equal(esperado, CuitValidator.FormatearSiEsCuit(valor));
    }

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
