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

namespace saas.Tests;

public class CategoriaControllerTests
{
    [Fact]
    public async Task CrearRapida_AdminEmpresaIgnoraEmpresaEnviada()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        CategoriaController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.CrearRapida(
            "Categoría nueva",
            empresaId: 2);

        Assert.IsType<JsonResult>(resultado);
        Categoria categoria = Assert.Single(context.Categorias);
        Assert.Equal(1, categoria.EmpresaId);
        Assert.Equal("Categoría nueva", categoria.Nombre);
        Assert.True(categoria.Estado);
    }

    [Fact]
    public async Task CrearRapida_NombreDuplicadoNoCreaOtraCategoria()
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararDatos(context);
        context.Categorias.Add(new Categoria
        {
            Nombre = "Almacén",
            EmpresaId = 1,
            Estado = true
        });
        await context.SaveChangesAsync();
        using UserManager<Usuario> userManager = CrearUserManager(context);
        CategoriaController controller = CrearController(context, userManager, usuario);

        IActionResult resultado = await controller.CrearRapida(
            "almacén",
            empresaId: null);

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Single(context.Categorias);
    }

    private static async Task<Usuario> PrepararDatos(SaasDbContext context)
    {
        context.Empresas.AddRange(
            new Empresa { Id = 1, Nombre = "Empresa A", Estado = true },
            new Empresa { Id = 2, Nombre = "Empresa B", Estado = true });
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

    private static CategoriaController CrearController(
        SaasDbContext context,
        UserManager<Usuario> userManager,
        Usuario usuario)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuario.Id)],
            "Prueba");
        var controller = new CategoriaController(context, userManager);
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
