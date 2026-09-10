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
using saas.ViewModel;

namespace saas.Tests;

public class MovimientoStockControllerTests
{
    [Fact]
    public async Task Historial_IdDeRutaInexistenteDevuelveNotFound()
    {
        // Evita interpretar /Historial/{id} como una consulta del historial general.
        await using var context = TestDbContextFactory.Crear();
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

        using UserManager<Usuario> userManager = CrearUserManager(context);
        MovimientoStockController controller =
            CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.Historial(
            999,
            new StockHistorialVM());

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Historial_FechaInvalidaNoEjecutaConsultaGeneral()
    {
        // Convierte el error de enlace en un mensaje claro y conserva la lista vacía.
        await using var context = TestDbContextFactory.Crear();
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

        using UserManager<Usuario> userManager = CrearUserManager(context);
        MovimientoStockController controller =
            CrearController(context, userManager, usuario);
        controller.ModelState.AddModelError(
            nameof(StockHistorialVM.FechaDesde),
            "The value 'incorrecta' is not valid.");

        IActionResult resultado = await controller.Historial(
            null,
            new StockHistorialVM());

        ViewResult vista = Assert.IsType<ViewResult>(resultado);
        StockHistorialVM modelo = Assert.IsType<StockHistorialVM>(vista.Model);
        Assert.Empty(modelo.Movimientos);
        Assert.Equal(
            "La fecha Desde no es válida.",
            controller.ModelState[nameof(StockHistorialVM.FechaDesde)]!
                .Errors.Single().ErrorMessage);
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

    private static MovimientoStockController CrearController(
        SaasDbContext context,
        UserManager<Usuario> userManager,
        Usuario usuario)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuario.Id)],
            "Prueba");
        var controller = new MovimientoStockController(
            context,
            userManager,
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
