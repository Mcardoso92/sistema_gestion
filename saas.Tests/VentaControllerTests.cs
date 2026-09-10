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
using saas.ViewModel;

namespace saas.Tests;

public class VentaControllerTests
{
    [Fact]
    public async Task Index_BusquedaExtensaMuestraErrorSinConsultarVentas()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await CrearUsuario(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        VentaController controller = CrearController(context, userManager, usuario);
        var modelo = new VentaIndexVM { Buscar = new string('a', 101) };

        IActionResult resultado = await controller.Index(modelo);

        ViewResult vista = Assert.IsType<ViewResult>(resultado);
        Assert.Same(modelo, vista.Model);
        Assert.Equal(
            "La búsqueda no puede superar los 100 caracteres.",
            controller.ModelState[nameof(VentaIndexVM.Buscar)]!
                .Errors.Single().ErrorMessage);
        Assert.Empty(modelo.Ventas);
    }

    [Fact]
    public async Task BuscarProductos_BusquedaExtensaDevuelveErrorControlado()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await CrearUsuario(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        VentaController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.BuscarProductos(
            new string('a', 101));

        BadRequestObjectResult error =
            Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Contains("100 caracteres", error.Value!.ToString());
    }

    [Fact]
    public async Task BuscarClientes_BusquedaExtensaDevuelveErrorControlado()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await CrearUsuario(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        VentaController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.BuscarClientes(
            new string('a', 101));

        BadRequestObjectResult error =
            Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Contains("100 caracteres", error.Value!.ToString());
    }

    private static async Task<Usuario> CrearUsuario(SaasDbContext context)
    {
        var usuario = new Usuario
        {
            Id = "usuario-a",
            UserName = "admin@a.com",
            NormalizedUserName = "ADMIN@A.COM",
            Nombre = "Admin",
            Apellido = "A",
            EmpresaId = 1,
            Estado = true
        };
        context.Users.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static UserManager<Usuario> CrearUserManager(
        SaasDbContext context)
    {
        var store = new UserStore<Usuario>(context);
        return new UserManager<Usuario>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<Usuario>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<Usuario>>.Instance);
    }

    private static VentaController CrearController(
        SaasDbContext context,
        UserManager<Usuario> userManager,
        Usuario usuario)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuario.Id)],
            "Prueba");
        var controller = new VentaController(
            context,
            userManager,
            new VentaSaldoService(context),
            new FechaHoraServicePrueba());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }
}
