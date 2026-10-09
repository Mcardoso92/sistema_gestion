using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class InventarioFisicoServiceTests
{
    [Fact]
    public async Task CrearAsync_GuardaStockTeoricoSinModificarProductos()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        InventarioFisicoService service = CrearService(context);

        InventarioFisico inventario = await service.CrearAsync(1, "usuario-1", "Conteo general", new[] { 1, 2 });

        Assert.Equal(EstadoInventarioFisico.Abierto, inventario.Estado);
        Assert.Equal(2, inventario.Detalles.Count);
        Assert.Contains(inventario.Detalles, d => d.ProductoId == 1 && d.StockTeorico == 10);
        Assert.Contains(inventario.Detalles, d => d.ProductoId == 2 && d.StockTeorico == 4);
        Assert.Equal(10, (await context.Productos.FindAsync(1))!.Stock);
        Assert.Empty(context.MovimientosStock);
    }

    [Fact]
    public async Task ConfirmarAsync_GeneraMovimientosCompensatoriosVinculados()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        InventarioFisicoService service = CrearService(context);
        InventarioFisico inventario = await service.CrearAsync(1, "usuario-1", "Conteo general", new[] { 1, 2 });
        Dictionary<int, int?> conteo = inventario.Detalles.ToDictionary(
            d => d.Id,
            d => (int?)(d.ProductoId == 1 ? 12 : 1));
        await service.GuardarConteoAsync(inventario.Id, 1, conteo);

        int ajustes = await service.ConfirmarAsync(inventario.Id, 1, "usuario-1");

        Assert.Equal(2, ajustes);
        Assert.Equal(12, (await context.Productos.FindAsync(1))!.Stock);
        Assert.Equal(1, (await context.Productos.FindAsync(2))!.Stock);
        List<MovimientoStock> movimientos = await context.MovimientosStock.ToListAsync();
        Assert.Contains(movimientos, m => m.ProductoId == 1 && m.Tipo == TipoMovimientoStock.AjusteEntrada && m.Cantidad == 2);
        Assert.Contains(movimientos, m => m.ProductoId == 2 && m.Tipo == TipoMovimientoStock.AjusteSalida && m.Cantidad == 3);
        Assert.All(movimientos, m => Assert.Equal(inventario.Id, m.InventarioFisicoId));
        Assert.Equal(EstadoInventarioFisico.Confirmado, inventario.Estado);
    }

    [Fact]
    public async Task ConfirmarAsync_BloqueaSiHuboMovimientosDuranteElConteo()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        InventarioFisicoService service = CrearService(context);
        InventarioFisico inventario = await service.CrearAsync(1, "usuario-1", "Conteo", new[] { 1 });
        DetalleInventarioFisico detalle = inventario.Detalles.Single();
        await service.GuardarConteoAsync(inventario.Id, 1, new Dictionary<int, int?> { [detalle.Id] = 10 });
        context.MovimientosStock.Add(new MovimientoStock
        {
            ProductoId = 1,
            EmpresaId = 1,
            UsuarioId = "usuario-1",
            Tipo = TipoMovimientoStock.AjusteEntrada,
            Cantidad = 1,
            StockAnterior = 10,
            StockPosterior = 11,
            Fecha = inventario.FechaInicio.AddSeconds(1)
        });
        await context.SaveChangesAsync();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConfirmarAsync(inventario.Id, 1, "usuario-1"));

        Assert.Contains("cambió mientras", error.Message);
        Assert.Equal(EstadoInventarioFisico.Abierto, inventario.Estado);
    }

    [Fact]
    public async Task CrearAsync_RechazaProductoDeOtraEmpresaYOtraSesionAbierta()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararDatos(context);
        InventarioFisicoService service = CrearService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearAsync(1, "usuario-1", "Inválido", new[] { 3 }));

        await service.CrearAsync(1, "usuario-1", "Primero", new[] { 1 });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearAsync(1, "usuario-1", "Segundo", new[] { 2 }));
    }

    private static InventarioFisicoService CrearService(SaasDbContext context) =>
        new(context, new StockProductoService(), new FechaHoraServicePrueba());

    private static async Task PrepararDatos(SaasDbContext context)
    {
        context.Empresas.AddRange(
            new Empresa { Id = 1, Nombre = "Empresa A", Estado = true },
            new Empresa { Id = 2, Nombre = "Empresa B", Estado = true });
        context.Categorias.AddRange(
            new Categoria { Id = 1, Nombre = "Categoría A", EmpresaId = 1, Estado = true },
            new Categoria { Id = 2, Nombre = "Categoría B", EmpresaId = 2, Estado = true });
        context.Users.Add(new Usuario { Id = "usuario-1", UserName = "admin@empresa.com", Nombre = "Admin", Apellido = "Empresa", EmpresaId = 1, Estado = true });
        context.Productos.AddRange(
            new Producto { Id = 1, Nombre = "Producto A", EmpresaId = 1, CategoriaId = 1, Estado = true, ControlaStock = true, Stock = 10 },
            new Producto { Id = 2, Nombre = "Producto B", EmpresaId = 1, CategoriaId = 1, Estado = true, ControlaStock = true, Stock = 4 },
            new Producto { Id = 3, Nombre = "Producto ajeno", EmpresaId = 2, CategoriaId = 2, Estado = true, ControlaStock = true, Stock = 8 });
        await context.SaveChangesAsync();
    }
}
