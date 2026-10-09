using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel.ProductoActualizacionMasiva;

namespace saas.Tests;

public class ProductoActualizacionMasivaServiceTests
{
    [Fact]
    public async Task PrepararVistaPrevia_FiltraEmpresaYCategoriaYRedondea()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        ProductoActualizacionMasivaService service = CrearService(context, cache);
        var modelo = new ProductoActualizacionMasivaVM
        {
            EmpresaId = 1,
            CategoriaId = 1,
            Campo = CampoAjustePrecioProducto.PrecioVenta,
            TipoAjuste = TipoAjustePrecioProducto.Porcentaje,
            ValorAjuste = 10,
            Motivo = "Nueva lista"
        };

        ProductoActualizacionMasivaVistaPreviaVM vista =
            await service.PrepararVistaPreviaAsync(modelo, 1, "usuario-1");

        ProductoActualizacionMasivaFilaVM fila = Assert.Single(vista.Productos);
        Assert.Equal("Producto A", fila.Nombre);
        Assert.Equal(123.46m, fila.VentaAnterior);
        Assert.Equal(135.81m, fila.VentaNueva);
        Assert.Equal(80m, fila.CostoNuevo);
        Assert.True(service.TryObtenerVistaPrevia(vista.Token, 1, "usuario-1", out _));
        Assert.False(service.TryObtenerVistaPrevia(vista.Token, 2, "usuario-1", out _));
    }

    [Fact]
    public async Task AplicarAsync_ModificaTodoElLoteYRegistraAuditoriaComun()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        ProductoActualizacionMasivaService service = CrearService(context, cache);
        var modelo = new ProductoActualizacionMasivaVM
        {
            EmpresaId = 1,
            Campo = CampoAjustePrecioProducto.Ambos,
            TipoAjuste = TipoAjustePrecioProducto.ImporteFijo,
            ValorAjuste = 5,
            Motivo = "Ajuste general"
        };
        ProductoActualizacionMasivaVistaPreviaVM vista =
            await service.PrepararVistaPreviaAsync(modelo, 1, "usuario-1");

        int cantidad = await service.AplicarAsync(vista.Token, 1, "usuario-1");

        Assert.Equal(2, cantidad);
        Producto productoA = await context.Productos.SingleAsync(p => p.Nombre == "Producto A");
        Assert.Equal(85m, productoA.PrecioCosto);
        Assert.Equal(128.46m, productoA.PrecioVenta);

        List<CambioValorProducto> cambios = await context.CambiosValorProducto.ToListAsync();
        Assert.Equal(4, cambios.Count);
        Assert.All(cambios, c => Assert.Equal(OrigenCambioValorProducto.ActualizacionMasiva, c.Origen));
        Assert.All(cambios, c => Assert.Equal("Ajuste general", c.Motivo));
        Assert.Single(cambios.Select(c => c.OperacionId).Distinct());
        Assert.False(service.TryObtenerVistaPrevia(vista.Token, 1, "usuario-1", out _));
    }

    [Fact]
    public async Task AplicarAsync_RechazaElLoteSiUnPrecioCambioLuegoDeLaVistaPrevia()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        ProductoActualizacionMasivaService service = CrearService(context, cache);
        var modelo = new ProductoActualizacionMasivaVM
        {
            EmpresaId = 1,
            Campo = CampoAjustePrecioProducto.PrecioVenta,
            TipoAjuste = TipoAjustePrecioProducto.Porcentaje,
            ValorAjuste = 10,
            Motivo = "Nueva lista"
        };
        ProductoActualizacionMasivaVistaPreviaVM vista =
            await service.PrepararVistaPreviaAsync(modelo, 1, "usuario-1");
        Producto producto = await context.Productos.SingleAsync(p => p.Nombre == "Producto A");
        producto.PrecioVenta = 999;
        await context.SaveChangesAsync();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AplicarAsync(vista.Token, 1, "usuario-1"));

        Assert.Contains("cambiaron desde la vista previa", error.Message);
        Assert.Empty(context.CambiosValorProducto);
        Assert.Equal(999, producto.PrecioVenta);
    }

    private static ProductoActualizacionMasivaService CrearService(
        SaasDbContext context,
        IMemoryCache cache) =>
        new(
            context,
            cache,
            new HistorialValorProductoService(context),
            new FechaHoraServicePrueba());

    private static async Task PrepararDatos(SaasDbContext context)
    {
        context.Empresas.AddRange(
            new Empresa { Id = 1, Nombre = "Empresa A", Estado = true },
            new Empresa { Id = 2, Nombre = "Empresa B", Estado = true });
        context.Categorias.AddRange(
            new Categoria { Id = 1, Nombre = "Categoría A", EmpresaId = 1, Estado = true },
            new Categoria { Id = 2, Nombre = "Categoría B", EmpresaId = 1, Estado = true },
            new Categoria { Id = 3, Nombre = "Otra empresa", EmpresaId = 2, Estado = true });
        context.Productos.AddRange(
            new Producto { Id = 1, Nombre = "Producto A", EmpresaId = 1, CategoriaId = 1, Estado = true, PrecioCosto = 80, PrecioVenta = 123.46m },
            new Producto { Id = 2, Nombre = "Producto B", EmpresaId = 1, CategoriaId = 2, Estado = true, PrecioCosto = 50, PrecioVenta = 75 },
            new Producto { Id = 3, Nombre = "Producto ajeno", EmpresaId = 2, CategoriaId = 3, Estado = true, PrecioCosto = 100, PrecioVenta = 150 });
        await context.SaveChangesAsync();
    }
}
