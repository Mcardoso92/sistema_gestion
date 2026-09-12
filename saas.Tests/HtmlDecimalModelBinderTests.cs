using saas.Configuracion;

namespace saas.Tests;

public class HtmlDecimalModelBinderTests
{
    [Theory]
    [InlineData("8094.4", 8094.4)]
    [InlineData("8427.2", 8427.2)]
    [InlineData("8000", 8000)]
    [InlineData("8000.50", 8000.50)]
    [InlineData("8000,50", 8000.50)]
    [InlineData("8.000,50", 8000.50)]
    [InlineData("0.01", 0.01)]
    public void TryParse_ImporteValido_ConservaMagnitud(
        string texto,
        double esperado)
    {
        bool resultado = HtmlDecimalModelBinder.TryParse(
            texto,
            out decimal valor);

        Assert.True(resultado);
        Assert.Equal((decimal)esperado, valor);
    }

    [Theory]
    [InlineData("80.944")]
    [InlineData("8000.123")]
    [InlineData("8,000")]
    [InlineData("importe")]
    public void TryParse_ImporteInvalidoOAmbiguo_EsRechazado(
        string texto)
    {
        bool resultado = HtmlDecimalModelBinder.TryParse(
            texto,
            out _);

        Assert.False(resultado);
    }
}
