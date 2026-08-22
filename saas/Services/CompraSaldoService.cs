using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models.Enums;

namespace saas.Services
{
    public class CompraSaldoService
    {
        private readonly SaasDbContext _context;

        public CompraSaldoService(
            SaasDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObtenerTotalPagado(
            int compraId)
        {
            return
                await _context.PagosProveedor
                    .AsNoTracking()
                    .Where(p =>
                        p.CompraId == compraId &&
                        p.Estado == EstadoPago.Activo)
                    .SumAsync(p =>
                        (decimal?)p.Importe)
                ?? 0;
        }

        public async Task<decimal> ObtenerSaldoPendiente(
            int compraId,
            decimal totalCompra)
        {
            decimal totalPagado =
                await ObtenerTotalPagado(
                    compraId);

            return Math.Max(
                0,
                totalCompra - totalPagado);
        }
    }
}