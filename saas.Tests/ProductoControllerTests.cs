using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using saas.Controllers;
using saas.Data;
using saas.Models;
using saas.Services;

namespace saas.Tests;

public class ProductoControllerTests
{
    [Fact]
    public async Task Index_AdminEmpresaNoPuedeForzarElFiltroDeOtraEmpresa()
    {
        // Impide que un administrador cambie EmpresaId manualmente para listar productos ajenos.
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        ProductoController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.Index("todos", empresaId: 2);

        ViewResult vista = Assert.IsType<ViewResult>(resultado);
        List<Producto> productos = Assert.IsAssignableFrom<IEnumerable<Producto>>(vista.Model).ToList();
        Producto producto = Assert.Single(productos);
        Assert.Equal(1, producto.EmpresaId);
    }

    [Fact]
    public async Task Details_AdminEmpresaNoPuedeAbrirProductoAjeno()
    {
        // Protege el acceso directo por URL cuando el ID pertenece a otra empresa.
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        ProductoController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.Details(2);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Details_AdminEmpresaPuedeAbrirProductoPropio()
    {
        // Confirma que el aislamiento no bloquee el acceso legítimo a productos de la empresa.
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        ProductoController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.Details(1);

        ViewResult vista = Assert.IsType<ViewResult>(resultado);
        Producto producto = Assert.IsType<Producto>(vista.Model);
        Assert.Equal(1, producto.Id);
        Assert.Equal(1, producto.EmpresaId);
    }

    [Fact]
    public async Task Edit_SinMotivoConservaStockRealYNoModificaElProducto()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        Producto productoDb = await context.Productos.FindAsync(1) ?? throw new InvalidOperationException();
        productoDb.Stock = 17;
        productoDb.PrecioCosto = 100;
        productoDb.UrlImagen = "/uploads/producto-a.webp";
        await context.SaveChangesAsync();

        using UserManager<Usuario> userManager = CrearUserManager(context);
        ProductoController controller = CrearController(context, userManager, usuario);
        var productoEnviado = new Producto
        {
            Id = 1,
            Nombre = productoDb.Nombre,
            CategoriaId = productoDb.CategoriaId,
            EmpresaId = productoDb.EmpresaId,
            Estado = productoDb.Estado,
            PrecioCosto = 120,
            PrecioVenta = productoDb.PrecioVenta,
            PuntoReposicion = productoDb.PuntoReposicion
        };

        IActionResult resultado = await controller.Edit(1, productoEnviado, null, motivoCambioCosto: null);

        ViewResult vista = Assert.IsType<ViewResult>(resultado);
        Producto productoMostrado = Assert.IsType<Producto>(vista.Model);
        Assert.Equal(17, productoMostrado.Stock);
        Assert.Equal("/uploads/producto-a.webp", productoMostrado.UrlImagen);
        Assert.True(controller.ModelState.ContainsKey("motivoCambioCosto"));

        context.ChangeTracker.Clear();
        Producto productoPersistido = await context.Productos.FindAsync(1) ?? throw new InvalidOperationException();
        Assert.Equal(17, productoPersistido.Stock);
        Assert.Equal(100, productoPersistido.PrecioCosto);
        Assert.Empty(context.CambiosCostoProducto);
    }

    private static async Task<Usuario> PrepararDatos(SaasDbContext context)
    {
        var empresaA = new Empresa { Id = 1, Nombre = "Empresa A", Estado = true };
        var empresaB = new Empresa { Id = 2, Nombre = "Empresa B", Estado = true };
        var categoriaA = new Categoria { Id = 1, Nombre = "Categoría A", EmpresaId = 1, Estado = true };
        var categoriaB = new Categoria { Id = 2, Nombre = "Categoría B", EmpresaId = 2, Estado = true };
        var usuario = new Usuario { Id = "usuario-a", UserName = "admin@a.com", NormalizedUserName = "ADMIN@A.COM", Nombre = "Admin", Apellido = "A", EmpresaId = 1, Estado = true };

        context.Empresas.AddRange(empresaA, empresaB);
        context.Categorias.AddRange(categoriaA, categoriaB);
        context.Productos.AddRange(
            new Producto { Id = 1, Nombre = "Producto A", EmpresaId = 1, CategoriaId = 1, Estado = true },
            new Producto { Id = 2, Nombre = "Producto B", EmpresaId = 2, CategoriaId = 2, Estado = true });
        context.Users.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static UserManager<Usuario> CrearUserManager(SaasDbContext context)
    {
        var store = new UserStore<Usuario>(context);
        return new UserManager<Usuario>(store, Options.Create(new IdentityOptions()), new PasswordHasher<Usuario>(), [], [], new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!, NullLogger<UserManager<Usuario>>.Instance);
    }

    private static ProductoController CrearController(SaasDbContext context, UserManager<Usuario> userManager, Usuario usuario)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, usuario.Id)], "Prueba");
        var controller = new ProductoController(context, userManager, new ImagenServicePrueba());
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        return controller;
    }

    private sealed class ImagenServicePrueba : IImagenService
    {
        public Task<ResultadoImagen> GuardarAsync(IFormFile archivo, int empresaId, string carpeta, string? identificador = null, string? rutaAnterior = null)
        {
            return Task.FromResult(new ResultadoImagen { Exito = true, Ruta = "/imagen-prueba.webp" });
        }

        public void Eliminar(string? ruta)
        {
        }
    }
}
