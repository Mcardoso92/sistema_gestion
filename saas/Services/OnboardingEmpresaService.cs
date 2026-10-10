using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.ViewModel.Dashboard;

namespace saas.Services
{
    public class OnboardingEmpresaService
    {
        private readonly SaasDbContext _context;

        public OnboardingEmpresaService(SaasDbContext context)
        {
            _context = context;
        }

        public async Task<OnboardingEmpresaVM?> ObtenerEstadoAsync(int empresaId)
        {
            return await _context.Empresas
                .AsNoTracking()
                .Where(e =>
                    e.Id == empresaId &&
                    !e.OnboardingFinalizado)
                .Select(e => new OnboardingEmpresaVM
                {
                    ProductoCreado = e.Productos.Any(p => p.Estado),
                    CajaAbierta = e.TurnosCaja.Any(),
                    VentaRegistrada = e.Ventas.Any(v => v.Estado)
                })
                .SingleOrDefaultAsync();
        }

        public async Task<bool> FinalizarAsync(int empresaId)
        {
            var empresa = await _context.Empresas
                .SingleOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
            {
                return false;
            }

            if (empresa.OnboardingFinalizado)
            {
                return true;
            }

            bool productoCreado = await _context.Productos.AnyAsync(p =>
                p.EmpresaId == empresaId &&
                p.Estado);
            bool cajaAbierta = await _context.TurnosCaja.AnyAsync(t =>
                t.EmpresaId == empresaId);
            bool ventaRegistrada = await _context.Ventas.AnyAsync(v =>
                v.EmpresaId == empresaId &&
                v.Estado);

            if (!productoCreado || !cajaAbierta || !ventaRegistrada)
            {
                return false;
            }

            empresa.OnboardingFinalizado = true;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
