using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using saas.Controllers;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel;

namespace saas.Tests;

public class VentaControllerTests
{
    [Theory]
    [InlineData(TipoMedioPago.Efectivo)]
    [InlineData(TipoMedioPago.Transferencia)]
    [InlineData(TipoMedioPago.TarjetaDebito)]
    [InlineData(TipoMedioPago.TarjetaCredito)]
    [InlineData(TipoMedioPago.QR)]
    [InlineData(TipoMedioPago.Cheque)]
    [InlineData(TipoMedioPago.Otro)]
    public async Task Create_CobroPerteneceAlTurnoYAfectaArqueoSoloSiCorresponde(
        TipoMedioPago tipoMedioPago)
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await CrearUsuario(context);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        await PrepararVentaConMedioNoEfectivo(
            context,
            usuario,
            tipoMedioPago);
        VentaController controller = CrearController(context, userManager, usuario);
        var modelo = new VentaCreateVM
        {
            Detalles =
            [
                new VentaDetalleCreateVM
                {
                    ProductoId = 1,
                    Cantidad = 1
                }
            ],
            Pagos =
            [
                new VentaPagoCreateVM
                {
                    MedioPagoId = 1,
                    CajaId = tipoMedioPago == TipoMedioPago.Efectivo ? 1 : 2,
                    Importe = 100,
                    ImporteRecibido = tipoMedioPago == TipoMedioPago.Efectivo
                        ? 100
                        : null
                }
            ]
        };

        IActionResult resultado = await controller.Create(modelo);

        Assert.True(
            resultado is RedirectToActionResult,
            string.Join(
                " | ",
                controller.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));
        CobroVenta cobro = await context.CobrosVenta.SingleAsync();
        MovimientoCaja movimiento = await context.MovimientosCaja.SingleAsync();
        Assert.Equal(10, cobro.TurnoCajaId);
        Assert.Equal(
            tipoMedioPago == TipoMedioPago.Efectivo ? 10 : null,
            movimiento.TurnoCajaId);
    }

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

    private static async Task PrepararVentaConMedioNoEfectivo(
        SaasDbContext context,
        Usuario usuario,
        TipoMedioPago tipoMedioPago)
    {
        var empresa = new Empresa
        {
            Id = 1,
            Nombre = "Empresa A",
            Estado = true
        };
        var categoria = new Categoria
        {
            Id = 1,
            Nombre = "Categoría",
            Estado = true,
            EmpresaId = empresa.Id
        };
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Producto",
            CategoriaId = categoria.Id,
            PrecioVenta = 100,
            Stock = 5,
            Estado = true,
            EmpresaId = empresa.Id
        };
        var cajaEfectivo = new Caja
        {
            Id = 1,
            Nombre = "Efectivo",
            Tipo = TipoCaja.Efectivo,
            PermiteTurnos = true,
            Estado = true,
            EmpresaId = empresa.Id
        };
        var cajaNoEfectivo = new Caja
        {
            Id = 2,
            Nombre = tipoMedioPago.ToString(),
            Tipo = TipoCaja.BilleteraVirtual,
            PermiteTurnos = false,
            Estado = true,
            EmpresaId = empresa.Id
        };
        var medioNoEfectivo = new MedioPago
        {
            Id = 1,
            Nombre = tipoMedioPago.ToString(),
            Tipo = tipoMedioPago,
            Estado = true,
            EmpresaId = empresa.Id
        };

        context.AddRange(
            empresa,
            categoria,
            producto,
            cajaEfectivo,
            cajaNoEfectivo,
            medioNoEfectivo,
            new CajaMedioPago
            {
                CajaId = tipoMedioPago == TipoMedioPago.Efectivo
                    ? cajaEfectivo.Id
                    : cajaNoEfectivo.Id,
                MedioPagoId = medioNoEfectivo.Id
            },
            new TurnoCaja
            {
                Id = 10,
                EmpresaId = empresa.Id,
                CajaId = cajaEfectivo.Id,
                UsuarioAperturaId = usuario.Id,
                Estado = EstadoTurnoCaja.Abierto
            });

        await context.SaveChangesAsync();
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
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(
            httpContext,
            new TempDataProviderPrueba());
        return controller;
    }

    private sealed class TempDataProviderPrueba : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(
            HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(
            HttpContext context,
            IDictionary<string, object> values)
        {
        }
    }
}
