using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class VentaSaldoServiceTests
{
    [Fact]
    public async Task ObtenerTotalCobrado_SumaSoloCobrosActivos()
    {
        // Protege la regla que excluye los cobros anulados del saldo de una venta.
        await using var context = TestDbContextFactory.Crear();
        context.CobrosVenta.AddRange(CrearCobro(1, 1, 600, EstadoCobro.Activo), CrearCobro(2, 1, 250, EstadoCobro.Anulado), CrearCobro(3, 2, 900, EstadoCobro.Activo));
        await context.SaveChangesAsync();

        var service = new VentaSaldoService(context);
        decimal resultado = await service.ObtenerTotalCobrado(1);

        Assert.Equal(600, resultado);
    }

    [Fact]
    public async Task ObtenerSaldoPendiente_NuncaDevuelveUnValorNegativo()
    {
        // Evita que un pago superior al total genere una deuda negativa para el cliente.
        await using var context = TestDbContextFactory.Crear();
        context.CobrosVenta.Add(CrearCobro(1, 1, 1200, EstadoCobro.Activo));
        await context.SaveChangesAsync();

        var service = new VentaSaldoService(context);
        decimal resultado = await service.ObtenerSaldoPendiente(1, 1000);

        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task PuedeAnularCobro_RechazaCuandoDejariaReintegrosSinRespaldo()
    {
        // Impide anular cobros si el importe restante sería menor que lo ya reintegrado.
        await using var context = TestDbContextFactory.Crear();
        context.CobrosVenta.Add(CrearCobro(1, 1, 1000, EstadoCobro.Activo));
        context.ReintegrosVenta.Add(CrearReintegro(1, 1, 700, EstadoReintegro.Activo));
        await context.SaveChangesAsync();

        var service = new VentaSaldoService(context);
        bool resultado = await service.PuedeAnularCobro(1, 400);

        Assert.False(resultado);
    }

    private static CobroVenta CrearCobro(int id, int ventaId, decimal importe, EstadoCobro estado)
    {
        return new CobroVenta { Id = id, VentaId = ventaId, Importe = importe, Estado = estado, UsuarioId = "usuario" };
    }

    private static ReintegroVenta CrearReintegro(int id, int ventaId, decimal importe, EstadoReintegro estado)
    {
        return new ReintegroVenta { Id = id, VentaId = ventaId, Importe = importe, Estado = estado, UsuarioId = "usuario" };
    }
}
