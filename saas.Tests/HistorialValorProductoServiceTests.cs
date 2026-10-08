using Microsoft.EntityFrameworkCore;
using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class HistorialValorProductoServiceTests
{
    [Fact]
    public void Registrar_UsaElMismoHistorialParaCostoYPrecioVenta()
    {
        using var context = TestDbContextFactory.Crear();
        var service = new HistorialValorProductoService(context);
        var producto = new Producto { Id = 7, EmpresaId = 3, Nombre = "Producto" };
        DateTime fecha = new(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc);
        Guid operacionId = Guid.NewGuid();

        service.Registrar(
            producto, "usuario", TipoValorProducto.Costo,
            100, 120, fecha, OrigenCambioValorProducto.EdicionManual, "Nuevo proveedor",
            operacionId: operacionId);
        service.Registrar(
            producto, "usuario", TipoValorProducto.PrecioVenta,
            180, 200, fecha, OrigenCambioValorProducto.EdicionManual, "Actualización comercial",
            operacionId: operacionId);

        CambioValorProducto[] cambios = context.ChangeTracker
            .Entries<CambioValorProducto>()
            .Select(e => e.Entity)
            .ToArray();

        Assert.Equal(2, cambios.Length);
        Assert.Contains(cambios, c => c.TipoValor == TipoValorProducto.Costo && c.ValorAnterior == 100 && c.ValorNuevo == 120);
        Assert.Contains(cambios, c => c.TipoValor == TipoValorProducto.PrecioVenta && c.ValorAnterior == 180 && c.ValorNuevo == 200);
        Assert.All(cambios, c => Assert.Equal(3, c.EmpresaId));
        Assert.All(cambios, c => Assert.Equal(operacionId, c.OperacionId));
    }

    [Fact]
    public void Registrar_NoAgregaEventoCuandoElValorNoCambio()
    {
        using var context = TestDbContextFactory.Crear();
        var service = new HistorialValorProductoService(context);
        var producto = new Producto { Id = 7, EmpresaId = 3, Nombre = "Producto" };

        service.Registrar(
            producto, "usuario", TipoValorProducto.PrecioVenta,
            200, 200, DateTime.UtcNow, OrigenCambioValorProducto.EdicionManual);

        Assert.Empty(context.ChangeTracker.Entries<CambioValorProducto>());
    }

    [Fact]
    public async Task RevertirAsync_RestauraElUltimoCambioManualYConservaLaAuditoria()
    {
        await using var context = TestDbContextFactory.Crear();
        var service = new HistorialValorProductoService(context);
        var producto = new Producto
        {
            Id = 7,
            EmpresaId = 3,
            Nombre = "Producto",
            PrecioCosto = 100,
            PrecioVenta = 250
        };
        var cambio = new CambioValorProducto
        {
            Producto = producto,
            EmpresaId = 3,
            UsuarioId = "usuario-original",
            TipoValor = TipoValorProducto.PrecioVenta,
            ValorAnterior = 200,
            ValorNuevo = 250,
            Fecha = new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc),
            Origen = OrigenCambioValorProducto.EdicionManual,
            Motivo = "Ajuste comercial"
        };
        context.Add(cambio);
        await context.SaveChangesAsync();

        ResultadoReversionValorProducto resultado = await service.RevertirAsync(
            cambio.Id,
            3,
            "usuario-reversion",
            new DateTime(2026, 10, 8, 13, 0, 0, DateTimeKind.Utc));

        Assert.True(resultado.Exito);
        Assert.Equal(200, producto.PrecioVenta);

        CambioValorProducto reversion = await context.CambiosValorProducto
            .SingleAsync(c => c.CambioRevertidoId == cambio.Id);
        Assert.Equal(OrigenCambioValorProducto.Reversion, reversion.Origen);
        Assert.Equal(250, reversion.ValorAnterior);
        Assert.Equal(200, reversion.ValorNuevo);
        Assert.Equal("usuario-reversion", reversion.UsuarioId);
    }

    [Fact]
    public async Task RevertirAsync_RechazaUnCambioQueYaTieneOtroPosterior()
    {
        await using var context = TestDbContextFactory.Crear();
        var service = new HistorialValorProductoService(context);
        var producto = new Producto
        {
            Id = 7,
            EmpresaId = 3,
            Nombre = "Producto",
            PrecioCosto = 100,
            PrecioVenta = 300
        };
        var cambioOriginal = new CambioValorProducto
        {
            Producto = producto,
            EmpresaId = 3,
            UsuarioId = "usuario",
            TipoValor = TipoValorProducto.PrecioVenta,
            ValorAnterior = 200,
            ValorNuevo = 250,
            Fecha = new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc),
            Origen = OrigenCambioValorProducto.EdicionManual
        };
        context.AddRange(
            cambioOriginal,
            new CambioValorProducto
            {
                Producto = producto,
                EmpresaId = 3,
                UsuarioId = "usuario",
                TipoValor = TipoValorProducto.PrecioVenta,
                ValorAnterior = 250,
                ValorNuevo = 300,
                Fecha = new DateTime(2026, 10, 8, 13, 0, 0, DateTimeKind.Utc),
                Origen = OrigenCambioValorProducto.EdicionManual
            });
        await context.SaveChangesAsync();

        ResultadoReversionValorProducto resultado = await service.RevertirAsync(
            cambioOriginal.Id,
            3,
            "usuario",
            DateTime.UtcNow);

        Assert.False(resultado.Exito);
        Assert.Equal(300, producto.PrecioVenta);
        Assert.DoesNotContain(
            context.CambiosValorProducto,
            c => c.Origen == OrigenCambioValorProducto.Reversion);
    }
}
