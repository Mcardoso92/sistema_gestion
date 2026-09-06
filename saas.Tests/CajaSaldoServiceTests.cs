using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Tests;

public class CajaSaldoServiceTests
{
    [Fact]
    public async Task CalcularSaldoDisponible_CajaSinTurnosSumaSoloSusMovimientos()
    {
        // Protege el aislamiento entre cajas al calcular ingresos menos egresos.
        await using var context = TestDbContextFactory.Crear();
        context.MovimientosCaja.AddRange(CrearMovimiento(1, 1, 500, DireccionMovimientoCaja.Ingreso), CrearMovimiento(2, 1, 120, DireccionMovimientoCaja.Egreso), CrearMovimiento(3, 2, 900, DireccionMovimientoCaja.Ingreso));
        await context.SaveChangesAsync();

        var service = new CajaSaldoService(context);
        var caja = new Caja { Id = 1, Nombre = "Principal", PermiteTurnos = false };
        decimal resultado = await service.CalcularSaldoDisponible(caja, "usuario");

        Assert.Equal(380, resultado);
    }

    [Fact]
    public async Task CalcularSaldoDisponible_CajaConTurnoUsaFondoYMovimientosDelUsuario()
    {
        // Asegura que cada cajero vea únicamente el saldo de su turno abierto más el fondo aplicado.
        await using var context = TestDbContextFactory.Crear();
        context.TurnosCaja.Add(new TurnoCaja { Id = 10, CajaId = 1, UsuarioAperturaId = "usuario-a", Estado = EstadoTurnoCaja.Abierto, FondoFijoAplicado = 100 });
        context.MovimientosCaja.AddRange(CrearMovimiento(1, 1, 500, DireccionMovimientoCaja.Ingreso, 10), CrearMovimiento(2, 1, 80, DireccionMovimientoCaja.Egreso, 10), CrearMovimiento(3, 1, 900, DireccionMovimientoCaja.Ingreso, 20));
        await context.SaveChangesAsync();

        var service = new CajaSaldoService(context);
        var caja = new Caja { Id = 1, Nombre = "Principal", PermiteTurnos = true };
        decimal resultado = await service.CalcularSaldoDisponible(caja, "usuario-a");

        Assert.Equal(520, resultado);
    }

    [Fact]
    public async Task CalcularSaldoDisponible_SinTurnoAbiertoDevuelveCero()
    {
        // Evita exponer saldo de caja a un usuario que no tiene un turno abierto.
        await using var context = TestDbContextFactory.Crear();
        var service = new CajaSaldoService(context);
        var caja = new Caja { Id = 1, Nombre = "Principal", PermiteTurnos = true };

        decimal resultado = await service.CalcularSaldoDisponible(caja, "usuario-sin-turno");

        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task CalcularSaldoDisponible_RecuperaTransferenciaEntranteSinTurnoDuranteTurnoActual()
    {
        // Mantiene disponibles los fondos de transferencias históricas que no se asociaron al turno abierto.
        await using var context = TestDbContextFactory.Crear();
        DateTime apertura = new(2026, 9, 4, 10, 0, 0);
        context.TurnosCaja.Add(new TurnoCaja { Id = 10, CajaId = 1, UsuarioAperturaId = "usuario-a", Estado = EstadoTurnoCaja.Abierto, FechaApertura = apertura });
        context.MovimientosCaja.AddRange(
            CrearMovimiento(1, 1, 100, DireccionMovimientoCaja.Ingreso, 10, TipoMovimientoCaja.IngresoManual, apertura.AddMinutes(1)),
            CrearMovimiento(2, 1, 100, DireccionMovimientoCaja.Egreso, 10, TipoMovimientoCaja.TransferenciaSalida, apertura.AddMinutes(2)),
            CrearMovimiento(3, 1, 100, DireccionMovimientoCaja.Ingreso, null, TipoMovimientoCaja.TransferenciaEntrada, apertura.AddMinutes(3)),
            CrearMovimiento(4, 1, 500, DireccionMovimientoCaja.Ingreso, null, TipoMovimientoCaja.TransferenciaEntrada, apertura.AddMinutes(-1)));
        await context.SaveChangesAsync();

        var service = new CajaSaldoService(context);
        var caja = new Caja { Id = 1, Nombre = "Mostrador", PermiteTurnos = true };

        decimal resultado = await service.CalcularSaldoDisponible(caja, "usuario-a");

        Assert.Equal(100, resultado);
    }

    private static MovimientoCaja CrearMovimiento(
        int id,
        int cajaId,
        decimal importe,
        DireccionMovimientoCaja direccion,
        int? turnoCajaId = null,
        TipoMovimientoCaja tipo = TipoMovimientoCaja.IngresoManual,
        DateTime? fecha = null)
    {
        return new MovimientoCaja { Id = id, CajaId = cajaId, Importe = importe, Direccion = direccion, TurnoCajaId = turnoCajaId, Tipo = tipo, Fecha = fecha ?? DateTime.Now, UsuarioId = "usuario" };
    }
}
