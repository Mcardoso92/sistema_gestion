using saas.Services;
using saas.Models.Enums;

namespace saas.Tests;

public class TextoPresentacionTests
{
    [Theory]
    [InlineData("SuperAdmin", "Superadministrador")]
    [InlineData("AdminEmpresa", "Administrador de empresa")]
    [InlineData("OtroRol", "OtroRol")]
    [InlineData(null, "")]
    public void Rol_DevuelveNombreAmigableSinModificarElValorInterno(string? rol, string esperado)
    {
        Assert.Equal(esperado, TextoPresentacion.Rol(rol));
    }

    [Theory]
    [InlineData(TipoMedioPago.TarjetaDebito, "Tarjeta de débito")]
    [InlineData(TipoMedioPago.TarjetaCredito, "Tarjeta de crédito")]
    [InlineData(TipoMedioPago.Efectivo, "Efectivo")]
    public void ValorEnum_DevuelveNombreConfigurado(TipoMedioPago tipo, string esperado)
    {
        Assert.Equal(esperado, TextoPresentacion.ValorEnum(tipo));
    }

    [Fact]
    public void ValorEnum_BilleteraVirtualDevuelveNombreAmigable()
    {
        Assert.Equal("Billetera virtual", TextoPresentacion.ValorEnum(TipoCaja.BilleteraVirtual));
    }

    [Theory]
    [InlineData(TipoMovimientoStock.StockInicial, "Stock inicial")]
    [InlineData(TipoMovimientoStock.AjusteEntrada, "Ajuste de entrada")]
    [InlineData(TipoMovimientoStock.AnulacionVenta, "Anulación de venta")]
    [InlineData(TipoMovimientoStock.ReintegroVenta, "Reintegro de venta")]
    [InlineData(TipoMovimientoStock.AnulacionDevolucionCompra, "Anulación de devolución de compra")]
    public void ValorEnum_MovimientoStockDevuelveNombreAmigable(TipoMovimientoStock tipo, string esperado)
    {
        Assert.Equal(esperado, TextoPresentacion.ValorEnum(tipo));
    }
}
