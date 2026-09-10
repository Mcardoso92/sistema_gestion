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

public class CobroVentaControllerTests
{
    [Theory]
    [InlineData(TipoMedioPago.Efectivo)]
    [InlineData(TipoMedioPago.Transferencia)]
    [InlineData(TipoMedioPago.TarjetaDebito)]
    [InlineData(TipoMedioPago.TarjetaCredito)]
    [InlineData(TipoMedioPago.QR)]
    [InlineData(TipoMedioPago.Cheque)]
    [InlineData(TipoMedioPago.Otro)]
    public async Task Registrar_CobroPerteneceAlTurnoYAfectaArqueoSoloSiCorresponde(
        TipoMedioPago tipoMedioPago)
    {
        await using var context = TestDbContextFactory.Crear();
        Usuario usuario = await PrepararEscenario(
            context,
            tipoMedioPago);
        using UserManager<Usuario> userManager = CrearUserManager(context);
        CobroVentaController controller = CrearController(
            context,
            userManager,
            usuario);
        var modelo = new RegistrarCobroVentaVM
        {
            VentaId = 1,
            MedioPagoId = 1,
            CajaId = tipoMedioPago == TipoMedioPago.Efectivo ? 1 : 2,
            Importe = 100
        };

        IActionResult resultado = await controller.Registrar(modelo);

        Assert.IsType<RedirectToActionResult>(resultado);
        CobroVenta cobro = await context.CobrosVenta.SingleAsync();
        MovimientoCaja movimiento = await context.MovimientosCaja.SingleAsync();
        Assert.Equal(10, cobro.TurnoCajaId);
        Assert.Equal(
            tipoMedioPago == TipoMedioPago.Efectivo ? 10 : null,
            movimiento.TurnoCajaId);

        var movimientoController = new MovimientoCajaController(
            context,
            userManager,
            new CajaSaldoService(context),
            new FechaHoraServicePrueba())
        {
            ControllerContext = controller.ControllerContext
        };

        ViewResult detalle = Assert.IsType<ViewResult>(
            await movimientoController.Details(movimiento.Id));
        MovimientoCajaDetailsVM detalleModelo =
            Assert.IsType<MovimientoCajaDetailsVM>(detalle.Model);
        Assert.Equal(10, detalleModelo.TurnoCajaId);
    }

    private static async Task<Usuario> PrepararEscenario(
        SaasDbContext context,
        TipoMedioPago tipoMedioPago)
    {
        var empresa = new Empresa
        {
            Id = 1,
            Nombre = "Empresa A",
            Estado = true
        };
        var usuario = new Usuario
        {
            Id = "usuario-a",
            UserName = "admin@a.com",
            NormalizedUserName = "ADMIN@A.COM",
            Nombre = "Admin",
            Apellido = "A",
            EmpresaId = empresa.Id,
            Estado = true
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
            usuario,
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
            },
            new Venta
            {
                Id = 1,
                EmpresaId = empresa.Id,
                UsuarioId = usuario.Id,
                Total = 100,
                Estado = true
            });

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

    private static CobroVentaController CrearController(
        SaasDbContext context,
        UserManager<Usuario> userManager,
        Usuario usuario)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, usuario.Id)],
            "Prueba");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        var controller = new CobroVentaController(
            context,
            userManager,
            new VentaSaldoService(context),
            new CajaSaldoService(context),
            new FechaHoraServicePrueba())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(
                httpContext,
                new TempDataProviderPrueba())
        };

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
