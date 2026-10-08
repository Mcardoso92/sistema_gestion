using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class StockProductoServiceTests
{
    private readonly StockProductoService _service = new();

    [Fact]
    public void TieneDisponible_NoExigeUnidadesCuandoElProductoNoControlaStock()
    {
        var producto = CrearProducto(controlaStock: false, stock: 0);

        bool resultado = _service.TieneDisponible(producto, 10);

        Assert.True(resultado);
    }

    [Fact]
    public void RegistrarSalida_NoModificaNiGeneraMovimientoCuandoNoControlaStock()
    {
        var producto = CrearProducto(controlaStock: false, stock: 0);

        MovimientoStock? movimiento = _service.RegistrarSalida(
            producto,
            2,
            1,
            TipoMovimientoStock.Venta,
            DateTime.UtcNow,
            "usuario");

        Assert.Null(movimiento);
        Assert.Equal(0, producto.Stock);
    }

    [Fact]
    public void RegistrarEntrada_ModificaYDocumentaElStockCuandoEstaControlado()
    {
        var producto = CrearProducto(controlaStock: true, stock: 3);

        MovimientoStock? movimiento = _service.RegistrarEntrada(
            producto,
            2,
            1,
            TipoMovimientoStock.AnulacionVenta,
            DateTime.UtcNow,
            "usuario");

        Assert.NotNull(movimiento);
        Assert.Equal(5, producto.Stock);
        Assert.Equal(3, movimiento.StockAnterior);
        Assert.Equal(5, movimiento.StockPosterior);
    }

    private static Producto CrearProducto(bool controlaStock, int stock)
    {
        return new Producto
        {
            Id = 1,
            Nombre = "Producto de prueba",
            ControlaStock = controlaStock,
            Stock = stock
        };
    }
}
