using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel.Dashboard;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class DashboardController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public DashboardController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            DateTime hoy = DateTime.Today;
            DateTime manana = hoy.AddDays(1);
            DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime inicioGrafico = hoy.AddDays(-6);

            IQueryable<Venta> ventas = _context.Ventas
                .AsNoTracking()
                .Where(v => v.Estado);

            IQueryable<Producto> productos = _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado);

            IQueryable<DetalleVenta> detalles = _context.DetallesVenta
                .AsNoTracking()
                .Where(d => d.Venta.Estado);

            if (!esSuperAdmin)
            {
                ventas = ventas.Where(v =>
                    v.EmpresaId == usuario.EmpresaId);

                productos = productos.Where(p =>
                    p.EmpresaId == usuario.EmpresaId);

                detalles = detalles.Where(d =>
                    d.Venta.EmpresaId == usuario.EmpresaId);
            }

            IQueryable<Venta> ventasDia = ventas.Where(v =>
                v.Fecha >= hoy &&
                v.Fecha < manana);

            IQueryable<Venta> ventasMes = ventas.Where(v =>
                v.Fecha >= inicioMes &&
                v.Fecha < manana);

            decimal totalVentasDia = await ventasDia
                .SumAsync(v => (decimal?)v.Total)
                ?? 0;

            int cantidadVentasDia = await ventasDia.CountAsync();

            decimal totalVentasMes = await ventasMes
                .SumAsync(v => (decimal?)v.Total)
                ?? 0;

            int cantidadVentasMes = await ventasMes.CountAsync();

            var productosStockBajo = await productos
                .Where(p => p.Stock <= p.PuntoReposicion)
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Nombre)
                .Take(5)
                .Select(p => new ProductoStockBajoVM
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarra = p.CodigoBarra,
                    Stock = p.Stock,
                    PuntoReposicion = p.PuntoReposicion
                })
                .ToListAsync();

            var productosMasVendidos = await detalles
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto.Nombre
                })
                .Select(g => new ProductoMasVendidoVM
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    ImporteVendido = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.CantidadVendida)
                .ThenBy(p => p.Nombre)
                .Take(5)
                .ToListAsync();

            var clientesAgrupados = await ventas
                .Where(v =>
                    v.ClienteId.HasValue &&
                    v.Cliente!.Estado)
                .GroupBy(v => new
                {
                    ClienteId = v.ClienteId!.Value,
                    v.Cliente!.Nombre,
                    v.Cliente.Apellido
                })
                .Select(g => new
                {
                    g.Key.ClienteId,
                    g.Key.Nombre,
                    g.Key.Apellido,
                    CantidadCompras = g.Count(),
                    ImporteComprado = g.Sum(v => v.Total)
                })
                .OrderByDescending(c => c.CantidadCompras)
                .ThenByDescending(c => c.ImporteComprado)
                .Take(5)
                .ToListAsync();

            var clientesFrecuentes = clientesAgrupados
                .Select(c => new ClienteFrecuenteVM
                {
                    ClienteId = c.ClienteId,
                    NombreCompleto = string.IsNullOrWhiteSpace(c.Apellido)
                        ? c.Nombre
                        : $"{c.Nombre} {c.Apellido}",
                    CantidadCompras = c.CantidadCompras,
                    ImporteComprado = c.ImporteComprado
                })
                .ToList();

            var ventasAgrupadas = await ventas
                .Where(v =>
                    v.Fecha >= inicioGrafico &&
                    v.Fecha < manana)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Sum(v => v.Total)
                })
                .ToListAsync();

            var ventasUltimosDias = Enumerable
                .Range(0, 7)
                .Select(indice =>
                {
                    DateTime fecha = inicioGrafico.AddDays(indice);

                    decimal total = ventasAgrupadas
                        .FirstOrDefault(v => v.Fecha == fecha)
                        ?.Total
                        ?? 0;

                    return new VentaDiariaVM
                    {
                        Fecha = fecha,
                        Total = total
                    };
                })
                .ToList();

            var vm = new DashboardVM
            {
                TotalVentasDia = totalVentasDia,
                CantidadVentasDia = cantidadVentasDia,
                TotalVentasMes = totalVentasMes,
                CantidadVentasMes = cantidadVentasMes,
                ProductosStockBajo = productosStockBajo,
                ProductosMasVendidos = productosMasVendidos,
                ClientesFrecuentes = clientesFrecuentes,
                VentasUltimosDias = ventasUltimosDias
            };

            ViewBag.EsVistaGlobal = esSuperAdmin;

            return View(vm);
        }
    }
}