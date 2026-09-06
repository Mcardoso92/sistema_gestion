using System.ComponentModel.DataAnnotations;
using saas.Models;
using saas.ViewModel;

namespace saas.Tests;

public class FinanzasValidacionTests
{
    [Fact]
    public void CobroVenta_ConImporteCeroEsInvalido()
    {
        // Impide registrar cobros sin un importe positivo aunque el request sea manipulado.
        var cobro = new CobroVenta { Importe = 0, UsuarioId = "usuario" };

        AssertImporteInvalido(cobro, nameof(CobroVenta.Importe));
    }

    [Fact]
    public void PagoProveedor_ConImporteNegativoEsInvalido()
    {
        // Impide registrar pagos negativos que alterarían artificialmente el saldo de la compra.
        var pago = new PagoProveedor { Importe = -1, UsuarioId = "usuario" };

        AssertImporteInvalido(pago, nameof(PagoProveedor.Importe));
    }

    [Fact]
    public void ReintegroProveedor_ConImporteCeroEsInvalido()
    {
        // Impide registrar reintegros sin valor económico mediante un POST manipulado.
        var reintegro = new ReintegroProveedor { Importe = 0, UsuarioId = "usuario" };

        AssertImporteInvalido(reintegro, nameof(ReintegroProveedor.Importe));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DetalleCompra_ConCostoNoPositivoEsInvalido(double costo)
    {
        // Impide registrar líneas sin costo aunque se omita la validación del navegador.
        var detalle = new DetalleCompraCreateVM
        {
            ProductoId = 1,
            Cantidad = 1,
            PrecioUnitario = (decimal)costo
        };

        AssertImporteInvalido(detalle, nameof(DetalleCompraCreateVM.PrecioUnitario));
    }

    [Fact]
    public void DetalleCompra_ConCostoPositivoEsValido()
    {
        var detalle = new DetalleCompraCreateVM
        {
            ProductoId = 1,
            Cantidad = 1,
            PrecioUnitario = 0.01m
        };
        var errores = new List<ValidationResult>();

        Validator.TryValidateObject(detalle, new ValidationContext(detalle), errores, true);

        Assert.DoesNotContain(errores, error =>
            error.MemberNames.Contains(nameof(DetalleCompraCreateVM.PrecioUnitario)));
    }

    private static void AssertImporteInvalido(object modelo, string propiedad)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), errores, true);
        Assert.Contains(errores, error => error.MemberNames.Contains(propiedad));
    }
}
