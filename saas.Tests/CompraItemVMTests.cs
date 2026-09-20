using saas.ViewModel;

namespace saas.Tests;

public class CompraItemVMTests
{
    [Fact]
    public void SinPagos_IndicaSaldoPendiente()
    {
        var compra = new CompraItemVM { Total = 1_000 };

        Assert.Equal(1_000, compra.SaldoPendiente);
        Assert.False(compra.EstaPagada);
        Assert.False(compra.TienePagoParcial);
    }

    [Fact]
    public void PagoParcial_ConservaElSaldoRestante()
    {
        var compra = new CompraItemVM { Total = 1_000, TotalPagado = 400 };

        Assert.Equal(600, compra.SaldoPendiente);
        Assert.False(compra.EstaPagada);
        Assert.True(compra.TienePagoParcial);
    }

    [Fact]
    public void PagoCompleto_ConsideraLasDevolucionesActivasEnElTotalNeto()
    {
        var compra = new CompraItemVM
        {
            Total = 1_000,
            TotalDevuelto = 250,
            TotalPagado = 750
        };

        Assert.Equal(750, compra.TotalNeto);
        Assert.Equal(0, compra.SaldoPendiente);
        Assert.True(compra.EstaPagada);
    }
}
