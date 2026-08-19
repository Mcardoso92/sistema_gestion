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
                    return 0;
                }

                decimal netoTurno =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .Where(m =>
                            m.TurnoCajaId == turno.Id)
                        .SumAsync(m =>
                            m.Direccion ==
                            DireccionMovimientoCaja.Ingreso
                                ? m.Importe
                                : -m.Importe);

                return turno.FondoFijoAplicado
                    + netoTurno;
            }

            return await _context.MovimientosCaja
                .AsNoTracking()
                .Where(m =>
                    m.CajaId == caja.Id)
                .SumAsync(m =>
                    m.Direccion ==
                    DireccionMovimientoCaja.Ingreso
                        ? m.Importe
                        : -m.Importe);
        }
    }
}