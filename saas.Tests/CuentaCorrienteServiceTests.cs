using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class CuentaCorrienteServiceTests
{
    [Fact]
    public async Task ObtenerClienteAsync_CalculaSaldoYConservaOperacionesAnuladasSinEfecto()
    {
        await using var context = TestDbContextFactory.Crear();
        AgregarEmpresaYCliente(context);
        context.Ventas.Add(new Venta
        {
            Id = 1, EmpresaId = 1, ClienteId = 1, UsuarioId = "u", Fecha = new DateTime(2026, 1, 1), Total = 100, Estado = true
        });
        context.CobrosVenta.AddRange(
            CrearCobro(1, 1, 40, EstadoCobro.Activo, new DateTime(2026, 1, 2)),
            CrearCobro(2, 1, 20, EstadoCobro.Anulado, new DateTime(2026, 1, 3)));
        context.ReintegrosVenta.Add(new ReintegroVenta
        {
            Id = 1, VentaId = 1, EmpresaId = 1, CajaId = 1, MedioPagoId = 1,
            UsuarioId = "u", Fecha = new DateTime(2026, 1, 4), Importe = 10, Estado = EstadoReintegro.Activo
        });
        await context.SaveChangesAsync();

        CuentaCorrienteService service = CrearServicio(context);
        var resultado = await service.ObtenerClienteAsync(1, 1, "todos", 1);

        Assert.NotNull(resultado);
        Assert.Equal(60, resultado.SaldoPendiente);
        Assert.Equal(4, resultado.TotalRegistros);
        Assert.Equal(60, resultado.Movimientos.Last().SaldoResultante);
        Assert.Equal(0, resultado.Movimientos.Single(m => m.Referencia.StartsWith("Cobro #2")).EfectoSaldo);
        Assert.Equal(0, resultado.Movimientos.Single(m => m.Tipo == "Reintegro").EfectoSaldo);
    }

    [Fact]
    public async Task ObtenerProveedorAsync_SeparaSaldoPendienteYSaldoARecuperar()
    {
        await using var context = TestDbContextFactory.Crear();
        AgregarEmpresaYProveedor(context);
        context.Compras.Add(new Compra
        {
            Id = 1, EmpresaId = 1, ProveedorId = 1, UsuarioId = "u", Fecha = new DateTime(2026, 1, 1), Total = 100, Estado = true
        });
        context.PagosProveedor.Add(new PagoProveedor
        {
            Id = 1, CompraId = 1, EmpresaId = 1, CajaId = 1, MedioPagoId = 1,
            UsuarioId = "u", Fecha = new DateTime(2026, 1, 2), Importe = 100, Estado = EstadoPago.Activo
        });
        context.DevolucionesCompra.Add(new DevolucionCompra
        {
            Id = 1, CompraId = 1, EmpresaId = 1, UsuarioId = "u",
            Fecha = new DateTime(2026, 1, 3), Total = 20, Estado = true
        });
        context.ReintegrosProveedor.Add(new ReintegroProveedor
        {
            Id = 1, CompraId = 1, EmpresaId = 1, CajaId = 1, MedioPagoId = 1,
            UsuarioId = "u", Fecha = new DateTime(2026, 1, 4), Importe = 5, Estado = EstadoReintegro.Activo
        });
        await context.SaveChangesAsync();

        CuentaCorrienteService service = CrearServicio(context);
        var resultado = await service.ObtenerProveedorAsync(1, 1, "todos", 1);

        Assert.NotNull(resultado);
        Assert.Equal(0, resultado.SaldoPendiente);
        Assert.Equal(15, resultado.SaldoARecuperar);
        Assert.Equal(-15, resultado.Movimientos.Last().SaldoResultante);
    }

    [Fact]
    public async Task Consultas_RechazanTitularesDeOtraEmpresa()
    {
        await using var context = TestDbContextFactory.Crear();
        AgregarEmpresaYCliente(context);
        AgregarEmpresaYProveedor(context);
        await context.SaveChangesAsync();

        CuentaCorrienteService service = CrearServicio(context);

        Assert.Null(await service.ObtenerClienteAsync(1, 2, "todos", 1));
        Assert.Null(await service.ObtenerProveedorAsync(1, 2, "todos", 1));
    }

    [Fact]
    public async Task ObtenerClienteAsync_PaginaLosMovimientos()
    {
        await using var context = TestDbContextFactory.Crear();
        AgregarEmpresaYCliente(context);

        for (int i = 1; i <= 21; i++)
        {
            context.Ventas.Add(new Venta
            {
                Id = i, EmpresaId = 1, ClienteId = 1, UsuarioId = "u",
                Fecha = new DateTime(2026, 1, 1).AddDays(i), Total = 1, Estado = true
            });
        }
        await context.SaveChangesAsync();

        CuentaCorrienteService service = CrearServicio(context);
        var resultado = await service.ObtenerClienteAsync(1, 1, "todos", 2);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalPaginas);
        Assert.Single(resultado.Movimientos);
        Assert.Equal(21, resultado.Movimientos[0].SaldoResultante);
    }

    private static CuentaCorrienteService CrearServicio(Data.SaasDbContext context) =>
        new(context, new VentaSaldoService(context), new CompraSaldoService(context));

    private static void AgregarEmpresaYCliente(Data.SaasDbContext context)
    {
        if (!context.Empresas.Local.Any(e => e.Id == 1) &&
            !context.Empresas.Any(e => e.Id == 1))
        {
            context.Empresas.Add(new Empresa { Id = 1, Nombre = "Empresa 1", Estado = true });
        }
        context.Clientes.Add(new Cliente
        {
            Id = 1, EmpresaId = 1, Nombre = "Cliente", Estado = true
        });
    }

    private static void AgregarEmpresaYProveedor(Data.SaasDbContext context)
    {
        if (!context.Empresas.Local.Any(e => e.Id == 1) &&
            !context.Empresas.Any(e => e.Id == 1))
        {
            context.Empresas.Add(new Empresa { Id = 1, Nombre = "Empresa 1", Estado = true });
        }
        context.Proveedores.Add(new Proveedor
        {
            Id = 1, EmpresaId = 1, RazonSocial = "Proveedor", Estado = true
        });
    }

    private static CobroVenta CrearCobro(
        int id,
        int ventaId,
        decimal importe,
        EstadoCobro estado,
        DateTime fecha) => new()
        {
            Id = id, VentaId = ventaId, EmpresaId = 1, CajaId = 1, MedioPagoId = 1,
            UsuarioId = "u", Fecha = fecha, Importe = importe, Estado = estado
        };
}
