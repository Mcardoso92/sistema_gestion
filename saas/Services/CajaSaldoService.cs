using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;

namespace saas.Services
{
    public class CajaSaldoService
    {
        private readonly SaasDbContext _context;

        public CajaSaldoService(
            SaasDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalcularSaldoDisponible(
            Caja caja,
            string usuarioId)
        {
            if (caja.PermiteTurnos)
            {
                var turno =
                    await _context.TurnosCaja
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t =>
                            t.CajaId == caja.Id &&
                            t.UsuarioAperturaId == usuarioId &&
                            t.Estado == EstadoTurnoCaja.Abierto);

                if (turno == null)
                {
                    bool hayOtroTurnoAbierto =
                        await _context.TurnosCaja
                            .AsNoTracking()
                            .AnyAsync(t =>
                                t.CajaId == caja.Id &&
                                t.Estado == EstadoTurnoCaja.Abierto);

                    if (hayOtroTurnoAbierto)
                    {
                        return 0;
                    }

                    return await CalcularSaldoContable(caja.Id);
                }

                decimal netoTurno =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .Where(m =>
                            m.TurnoCajaId == turno.Id ||
                            (m.CajaId == caja.Id &&
                             m.TurnoCajaId == null &&
                             m.Fecha >= turno.FechaApertura &&
                             (m.Tipo == TipoMovimientoCaja.TransferenciaEntrada ||
                              m.Tipo == TipoMovimientoCaja.ReversionTransferenciaEntrada)))
                        .SumAsync(m =>
                            m.Direccion ==
                            DireccionMovimientoCaja.Ingreso
                                ? m.Importe
                                : -m.Importe);

                return turno.FondoFijoAplicado
                    + netoTurno;
            }

            return await CalcularSaldoContable(caja.Id);
        }

        private async Task<decimal> CalcularSaldoContable(int cajaId)
        {
            return await _context.MovimientosCaja
                .AsNoTracking()
                .Where(m =>
                    m.CajaId == cajaId)
                .SumAsync(m =>
                    m.Direccion ==
                    DireccionMovimientoCaja.Ingreso
                        ? m.Importe
                        : -m.Importe);
        }
    }
}
