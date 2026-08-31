using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class CompraSaldoServiceTests
{
    [Fact]
    public async Task ObtenerTotalPagado_SumaSoloPagosActivos()
    {
        // Evita que un pago anulado siga reduciendo el saldo pendiente de la compra.
        await using var context = TestDbContextFactory.Crear();
        context.PagosProveedor.AddRange(CrearPago(1, 1, 600, EstadoPago.Activo), CrearPago(2, 1, 250, EstadoPago.Anulado), CrearPago(3, 2, 900, EstadoPago.Activo));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerTotalPagado(1);

        Assert.Equal(600, resultado);
    }

    [Fact]
    public async Task ObtenerTotalNetoCompra_RestaSoloDevolucionesActivas()
    {
        // Protege el total neto para que las devoluciones anuladas no reduzcan la compra.
        await using var context = TestDbContextFactory.Crear();
        context.DevolucionesCompra.AddRange(CrearDevolucion(1, 1, 300, true), CrearDevolucion(2, 1, 200, false));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerTotalNetoCompra(1, 1000);

        Assert.Equal(700, resultado);
    }

    [Fact]
    public async Task ObtenerSaldoPendiente_NuncaDevuelveUnValorNegativo()
    {
        // Evita que un pago superior al total neto genere una deuda negativa al proveedor.
        await using var context = TestDbContextFactory.Crear();
        context.PagosProveedor.Add(CrearPago(1, 1, 1200, EstadoPago.Activo));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerSaldoPendiente(1, 1000);

        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task ObtenerPendienteRecuperar_DescuentaReintegrosActivos()
    {
        // Calcula cuánto dinero falta recuperar después de una devolución y un reintegro parcial.
        await using var context = TestDbContextFactory.Crear();
        context.PagosProveedor.Add(CrearPago(1, 1, 1000, EstadoPago.Activo));
        context.DevolucionesCompra.Add(CrearDevolucion(1, 1, 400, true));
        context.ReintegrosProveedor.Add(CrearReintegro(1, 1, 150, EstadoReintegro.Activo));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerPendienteRecuperar(1, 1000);

        Assert.Equal(250, resultado);
    }

    [Fact]
    public async Task ObtenerTotalNetoCompra_NuncaDevuelveUnValorNegativo()
    {
        // Evita totales negativos si las devoluciones acumuladas alcanzan o superan la compra.
        await using var context = TestDbContextFactory.Crear();
        context.DevolucionesCompra.Add(CrearDevolucion(1, 1, 1200, true));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerTotalNetoCompra(1, 1000);

        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task ObtenerTotalReintegrado_SumaSoloReintegrosActivos()
    {
        // Excluye los reintegros anulados del dinero efectivamente recuperado.
        await using var context = TestDbContextFactory.Crear();
        context.ReintegrosProveedor.AddRange(CrearReintegro(1, 1, 150, EstadoReintegro.Activo), CrearReintegro(2, 1, 100, EstadoReintegro.Anulado));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerTotalReintegrado(1);

        Assert.Equal(150, resultado);
    }

    [Fact]
    public async Task ObtenerPendienteRecuperar_SinSobrepagoDevuelveCero()
    {
        // Confirma que no exista dinero por recuperar cuando lo pagado no supera el total neto.
        await using var context = TestDbContextFactory.Crear();
        context.PagosProveedor.Add(CrearPago(1, 1, 500, EstadoPago.Activo));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerPendienteRecuperar(1, 1000);

        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task ObtenerPendienteRecuperar_NuncaDevuelveUnValorNegativo()
    {
        // Evita un pendiente negativo cuando los reintegros alcanzan el exceso pagado.
        await using var context = TestDbContextFactory.Crear();
        context.PagosProveedor.Add(CrearPago(1, 1, 1000, EstadoPago.Activo));
        context.DevolucionesCompra.Add(CrearDevolucion(1, 1, 300, true));
        context.ReintegrosProveedor.Add(CrearReintegro(1, 1, 400, EstadoReintegro.Activo));
        await context.SaveChangesAsync();

        var service = new CompraSaldoService(context);
        decimal resultado = await service.ObtenerPendienteRecuperar(1, 1000);

        Assert.Equal(0, resultado);
    }

    private static PagoProveedor CrearPago(int id, int compraId, decimal importe, EstadoPago estado)
    {
        return new PagoProveedor { Id = id, CompraId = compraId, Importe = importe, Estado = estado, UsuarioId = "usuario" };
    }

    private static DevolucionCompra CrearDevolucion(int id, int compraId, decimal total, bool estado)
    {
        return new DevolucionCompra { Id = id, CompraId = compraId, Total = total, Estado = estado, UsuarioId = "usuario" };
    }

    private static ReintegroProveedor CrearReintegro(int id, int compraId, decimal importe, EstadoReintegro estado)
    {
        return new ReintegroProveedor { Id = id, CompraId = compraId, Importe = importe, Estado = estado, UsuarioId = "usuario" };
    }
}
