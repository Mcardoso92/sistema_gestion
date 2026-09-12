using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

public class NotificacionControllerTests
{
    [Fact]
    public async Task MarcarComoVistas_AdminRegistraSoloSuEmpresaYEsIdempotente()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, Empresa empresa, _) = await PrepararDatos(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        var fechaHora = new FechaHoraServicePrueba(
            new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc));
        NotificacionController controller = CrearController(
            context,
            userManager,
            usuario,
            fechaHora);

        IActionResult primerResultado = await controller.MarcarComoVistas();
        fechaHora.UtcAhora = fechaHora.UtcAhora.AddMinutes(5);
        IActionResult segundoResultado = await controller.MarcarComoVistas();

        Assert.IsType<NoContentResult>(primerResultado);
        Assert.IsType<NoContentResult>(segundoResultado);
        RevisionNotificacion revision =
            Assert.Single(context.RevisionesNotificacion);
        Assert.Equal(usuario.Id, revision.UsuarioId);
        Assert.Equal(empresa.Id, revision.EmpresaId);
        Assert.Equal(fechaHora.UtcAhora, revision.FechaUltimaRevision);
    }

    [Fact]
    public async Task MarcarComoVistas_SuperAdminRegistraTodasLasEmpresas()
    {
        await using SaasDbContext context = TestDbContextFactory.Crear();
        (Usuario usuario, _, Empresa otraEmpresa) = await PrepararDatos(context);
        var rol = new IdentityRole
        {
            Id = "rol-superadmin",
            Name = "SuperAdmin",
            NormalizedName = "SUPERADMIN"
        };
        context.Roles.Add(rol);
        context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = usuario.Id,
            RoleId = rol.Id
        });
        await context.SaveChangesAsync();
        using UserManager<Usuario> userManager = CrearUserManager(context);
        NotificacionController controller = CrearController(
            context,
            userManager,
            usuario,
            new FechaHoraServicePrueba(DateTime.UtcNow));

        IActionResult resultado = await controller.MarcarComoVistas();

        Assert.IsType<NoContentResult>(resultado);
        Assert.Equal(2, context.RevisionesNotificacion.Count());
        Assert.Contains(
            context.RevisionesNotificacion,
            r => r.EmpresaId == otraEmpresa.Id);
    }

    [Fact]
    public void MarcarComoVistas_RequiereAutenticacionYAntifalsificacion()
    {
        MethodInfo metodo = typeof(NotificacionController)
            .GetMethod(nameof(NotificacionController.MarcarComoVistas))!;

        Assert.NotNull(typeof(NotificacionController)
            .GetCustomAttribute<AuthorizeAttribute>());
        Assert.NotNull(metodo.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(metodo.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
    }

    private static async Task<(Usuario Usuario, Empresa Empresa, Empresa OtraEmpresa)>
        PrepararDatos(SaasDbContext context)
    {
        var empresa = new Empresa
        {
            Nombre = "Empresa del usuario",
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        var otraEmpresa = new Empresa
        {
            Nombre = "Otra empresa",
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        context.Empresas.AddRange(empresa, otraEmpresa);
        await context.SaveChangesAsync();

        var usuario = new Usuario
        {
            Id = "usuario-notificaciones",
            UserName = "avisos@empresa.com",
            NormalizedUserName = "AVISOS@EMPRESA.COM",
            Nombre = "Usuario",
            Apellido = "Prueba",
            EmpresaId = empresa.Id,
            Estado = true,
            FechaAlta = DateTime.UtcNow
        };
        context.Users.Add(usuario);
        await context.SaveChangesAsync();

        return (usuario, empresa, otraEmpresa);
    }

    private static UserManager<Usuario> CrearUserManager(SaasDbContext context)
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

    private static NotificacionController CrearController(
        SaasDbContext context,
        UserManager<Usuario> userManager,
        Usuario usuario,
        IFechaHoraService fechaHora)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuario.Id)],
            "Prueba");
        var service = new RevisionNotificacionService(context, fechaHora);
        var controller = new NotificacionController(
            context,
            userManager,
            service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
        return controller;
    }

    private sealed class FechaHoraServicePrueba(DateTime utcAhora) : IFechaHoraService
    {
        public DateTime UtcAhora { get; set; } = utcAhora;
        public DateTime HoraLocalAhora => UtcAhora;
        public DateTime FechaLocalHoy => UtcAhora.Date;
        public DateTime ConvertirAHoraLocal(DateTime fechaUtc) => fechaUtc;
        public DateTime? ConvertirAHoraLocal(DateTime? fechaUtc) => fechaUtc;
        public DateTime ConvertirAUtc(DateTime fechaLocal) => fechaLocal;
    }
}
