using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models.Enums;

namespace saas.Services
{
    public class VentaSaldoService
    {
        private readonly SaasDbContext _context;

        public VentaSaldoService(
            SaasDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObtenerTotalCobrado(
            int ventaId)
        {
            return
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.VentaId == ventaId &&
                        c.Estado == EstadoCobro.Activo)
                    .SumAsync(c =>
                        (decimal?)c.Importe)
                ?? 0;
        }

        public async Task<decimal> ObtenerSaldoPendiente(
            int ventaId,
            decimal totalVenta)
        {
            decimal totalCobrado =
                await ObtenerTotalCobrado(
                    ventaId);

            return Math.Max(
                0,
                totalVenta - totalCobrado);
        }

        public async Task<decimal> ObtenerTotalReintegrado(
            int ventaId)
        {
            return
                await _context.ReintegrosVenta
                    .AsNoTracking()
                    .Where(r =>
                        r.VentaId == ventaId &&
                        r.Estado == EstadoReintegro.Activo)
                    .SumAsync(r =>
                        (decimal?)r.Importe)
                ?? 0;
        }

        public async Task<decimal> ObtenerImporteDisponibleReintegro(
            int ventaId)
        {
            decimal totalCobrado =
                await ObtenerTotalCobrado(
                    ventaId);

            decimal totalReintegrado =
                await ObtenerTotalReintegrado(
                    ventaId);

            return Math.Max(
                0,
                totalCobrado - totalReintegrado);
        }
    }
}