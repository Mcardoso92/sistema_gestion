using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models.Enums;

namespace saas.Services
{
    public sealed record PosicionProveedor(
        decimal SaldoPendiente,
        decimal SaldoARecuperar);

    public class CompraSaldoService
    {
        private readonly SaasDbContext _context;

        public CompraSaldoService(
            SaasDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObtenerTotalPagado(int compraId)
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
        public async Task<decimal> ObtenerTotalDevuelto(int compraId)
        {
            return
                await _context.DevolucionesCompra
                    .AsNoTracking()
                    .Where(d =>
                        d.CompraId == compraId &&
                        d.Estado)
                    .SumAsync(d =>
                        (decimal?)d.Total)
                ?? 0;
        }
        public async Task<decimal> ObtenerTotalNetoCompra(int compraId, decimal totalCompra)
        {
            decimal totalDevuelto =
                await ObtenerTotalDevuelto(
                    compraId);

            return Math.Max(
                0,
                totalCompra - totalDevuelto);
        }

        public async Task<decimal> ObtenerSaldoPendiente(int compraId, decimal totalCompra)
        {
            decimal totalNetoCompra =
                await ObtenerTotalNetoCompra(
                    compraId,
                    totalCompra);

            decimal totalPagado =
                await ObtenerTotalPagado(
                    compraId);

            return Math.Max(
                0,
                totalNetoCompra - totalPagado);
        }
        public async Task<decimal> ObtenerTotalReintegrado(int compraId)
        {
            return
                await _context.ReintegrosProveedor
                    .AsNoTracking()
                    .Where(r =>
                        r.CompraId == compraId &&
                        r.Estado == EstadoReintegro.Activo)
                    .SumAsync(r =>
                        (decimal?)r.Importe)
                ?? 0;
        }
        public async Task<decimal> ObtenerPendienteRecuperar(int compraId, decimal totalCompra)
        {
            decimal totalNetoCompra =
                await ObtenerTotalNetoCompra(
                    compraId,
                    totalCompra);

            decimal totalPagado =
                await ObtenerTotalPagado(
                    compraId);

            decimal totalReintegrado =
                await ObtenerTotalReintegrado(
                    compraId);

            decimal excesoPagado =
                Math.Max(
                    0,
                    totalPagado - totalNetoCompra);

            return Math.Max(
                0,
                excesoPagado - totalReintegrado);
        }

        public async Task<PosicionProveedor> ObtenerPosicionProveedor(
            int proveedorId,
            int empresaId)
        {
            var posiciones = await _context.Compras
                .AsNoTracking()
                .Where(c =>
                    c.ProveedorId == proveedorId &&
                    c.EmpresaId == empresaId &&
                    c.Estado)
                .Select(c => new
                {
                    c.Total,
                    TotalDevuelto = c.DevolucionesCompra
                        .Where(d => d.Estado)
                        .Sum(d => (decimal?)d.Total) ?? 0,
                    TotalPagado = c.PagosProveedor
                        .Where(p => p.Estado == EstadoPago.Activo)
                        .Sum(p => (decimal?)p.Importe) ?? 0,
                    TotalReintegrado = c.ReintegrosProveedor
                        .Where(r => r.Estado == EstadoReintegro.Activo)
                        .Sum(r => (decimal?)r.Importe) ?? 0
                })
                .ToListAsync();

            decimal saldoPendiente = 0;
            decimal saldoARecuperar = 0;

            foreach (var posicion in posiciones)
            {
                decimal totalNeto = Math.Max(0, posicion.Total - posicion.TotalDevuelto);
                saldoPendiente += Math.Max(0, totalNeto - posicion.TotalPagado);
                saldoARecuperar += Math.Max(
                    0,
                    posicion.TotalPagado - totalNeto - posicion.TotalReintegrado);
            }

            return new PosicionProveedor(saldoPendiente, saldoARecuperar);
        }
    }
}
