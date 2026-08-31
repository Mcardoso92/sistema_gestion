using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel.ProductoImportacion;

namespace saas.Tests;

public class ProductoImportacionServiceTests
{
    [Fact]
    public void GenerarPlantilla_CreaLasColumnasEsperadas()
    {
        // Protege el contrato de columnas que necesitan el lector y los archivos completados por los usuarios.
        using SaasDbContext context = TestDbContextFactory.Crear();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductoImportacionService(context, cache);

        byte[] archivo = service.GenerarPlantilla();

        using var libro = new XLWorkbook(new MemoryStream(archivo));
        IXLWorksheet hoja = libro.Worksheet("Productos");
        string[] encabezados = hoja.Row(1).Cells(1, 8).Select(c => c.GetString()).ToArray();
        Assert.Equal(new[] { "Nombre", "CodigoBarra", "Categoria", "PrecioCosto", "PrecioVenta", "StockInicial", "PuntoReposicion", "Descripcion" }, encabezados);
    }

    [Fact]
    public async Task AnalizarArchivoConError_BloqueaTodaLaImportacion()
    {
        // Confirma la modalidad todo-o-nada cuando una fila contiene un precio inválido.
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararEmpresa(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductoImportacionService(context, cache);
        IFormFile archivo = CrearArchivo(service, new object?[][]
        {
            new object?[] { "Producto válido", "COD-001", null, 100m, 150m, 10, 3, null },
            new object?[] { "Producto inválido", "COD-002", null, 100m, -1m, 5, 2, null }
        });

        ProductoImportacionVistaPreviaVM vistaPrevia = await service.AnalizarAsync(archivo, 1, "usuario-1");

        Assert.Equal(2, vistaPrevia.TotalFilas);
        Assert.Equal(1, vistaPrevia.FilasConErrores);
        Assert.False(vistaPrevia.PuedeImportar);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ImportarAsync(vistaPrevia.Token, 1, "usuario-1"));
        Assert.Empty(context.Productos);
    }

    [Fact]
    public async Task ImportarArchivoValido_CreaProductoYMovimientoDeStock()
    {
        // Verifica que la confirmación asocie la empresa, use Sin categoría y conserve la trazabilidad del stock inicial.
        await using SaasDbContext context = TestDbContextFactory.Crear();
        await PrepararEmpresa(context);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductoImportacionService(context, cache);
        IFormFile archivo = CrearArchivo(service, new object?[][]
        {
            new object?[] { "Producto importado", "COD-100", null, 100m, 150m, 10, 3, "Prueba automatizada" }
        });

        ProductoImportacionVistaPreviaVM vistaPrevia = await service.AnalizarAsync(archivo, 1, "usuario-1");
        int cantidad = await service.ImportarAsync(vistaPrevia.Token, 1, "usuario-1");

        Producto producto = await context.Productos.SingleAsync();
        MovimientoStock movimiento = await context.MovimientosStock.SingleAsync();
        Assert.Equal(1, cantidad);
        Assert.Equal(1, producto.EmpresaId);
        Assert.Equal("Sin categoría", vistaPrevia.Filas.Single().Categoria);
        Assert.Equal(10, producto.Stock);
        Assert.Equal(producto.Id, movimiento.ProductoId);
        Assert.Equal("Stock inicial por importación", movimiento.Motivo);
        Assert.False(service.TryObtenerVistaPrevia(vistaPrevia.Token, 1, "usuario-1", out _));
    }

    private static IFormFile CrearArchivo(ProductoImportacionService service, object?[][] filas)
    {
        byte[] plantilla = service.GenerarPlantilla();
        using var libro = new XLWorkbook(new MemoryStream(plantilla));
        IXLWorksheet hoja = libro.Worksheet("Productos");

        for (int fila = 0; fila < filas.Length; fila++)
        {
            for (int columna = 0; columna < filas[fila].Length; columna++)
            {
                hoja.Cell(fila + 2, columna + 1).Value = XLCellValue.FromObject(filas[fila][columna]);
            }
        }

        var flujo = new MemoryStream();
        libro.SaveAs(flujo);
        flujo.Position = 0;
        return new FormFile(flujo, 0, flujo.Length, "Archivo", "productos.xlsx");
    }

    private static async Task PrepararEmpresa(SaasDbContext context)
    {
        context.Empresas.Add(new Empresa { Id = 1, Nombre = "Empresa A", Estado = true });
        context.Categorias.Add(new Categoria { Id = 1, Nombre = "Sin categoría", EmpresaId = 1, Estado = true });
        context.Users.Add(new Usuario { Id = "usuario-1", UserName = "admin@empresa.com", Nombre = "Admin", Apellido = "Empresa", EmpresaId = 1, Estado = true });
        await context.SaveChangesAsync();
    }
}
