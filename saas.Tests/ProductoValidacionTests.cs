using System.ComponentModel.DataAnnotations;
using saas.Models;

namespace saas.Tests;

public class ProductoValidacionTests
{
    [Fact]
    public void Producto_SinNombreEsInvalido()
    {
        // Evita crear productos sin el dato principal requerido para identificarlos.
        var producto = new Producto { Nombre = "", PrecioCosto = 10, PrecioVenta = 20, Stock = 1, PuntoReposicion = 1 };

        List<ValidationResult> errores = Validar(producto);

        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Producto.Nombre)));
    }

    [Fact]
    public void Producto_ConValoresNegativosEsInvalido()
    {
        // Protege precios, stock y punto de reposición contra valores negativos manipulados.
        var producto = new Producto { Nombre = "Producto", PrecioCosto = -1, PrecioVenta = -1, Stock = -1, PuntoReposicion = -1 };

        List<ValidationResult> errores = Validar(producto);

        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Producto.PrecioCosto)));
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Producto.PrecioVenta)));
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Producto.Stock)));
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Producto.PuntoReposicion)));
    }

    [Fact]
    public void Producto_ConDatosValidosNoGeneraErrores()
    {
        // Confirma que las validaciones permitan guardar un producto correctamente formado.
        var producto = new Producto { Nombre = "Producto", PrecioCosto = 100, PrecioVenta = 150, Stock = 10, PuntoReposicion = 2 };

        List<ValidationResult> errores = Validar(producto);

        Assert.Empty(errores);
    }

    private static List<ValidationResult> Validar(Producto producto)
    {
        var errores = new List<ValidationResult>();
        Validator.TryValidateObject(producto, new ValidationContext(producto), errores, true);
        return errores;
    }
}
