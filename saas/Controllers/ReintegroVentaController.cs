using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ReintegroVentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ReintegroVentaController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ReintegroVenta/Registrar?ventaId=5
        [HttpGet]
        public async Task<IActionResult> Registrar(int ventaId)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<Venta> consulta =
                _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(v =>
                        v.EmpresaId == usuario.EmpresaId);
            }

            var venta =
                await consulta
                    .FirstOrDefaultAsync(v =>
                        v.Id == ventaId);

            if (venta == null)
            {
                return NotFound();
            }

            if (!venta.Estado)
            {
                TempData["Error"] =
                    "No se pueden registrar reintegros sobre una venta anulada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            decimal totalCobrado =
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.VentaId == venta.Id &&
                        c.Estado == EstadoCobro.Activo)
                    .SumAsync(c =>
                        (decimal?)c.Importe)
                ?? 0;

            decimal totalReintegrado =
                await _context.ReintegrosVenta
                    .AsNoTracking()
                    .Where(r =>
                        r.VentaId == venta.Id &&
                        r.Estado == EstadoReintegro.Activo)
                    .SumAsync(r =>
                        (decimal?)r.Importe)
                ?? 0;

            decimal importeDisponible =
                Math.Max(
                    0,
                    totalCobrado - totalReintegrado);

            if (importeDisponible <= 0)
            {
                TempData["Error"] =
                    "La venta no tiene importe disponible para reintegrar.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            var cantidadesReintegradas =
                await _context.DetallesReintegroVenta
                    .AsNoTracking()
                    .Where(d =>
                        d.ReintegroVenta.VentaId == venta.Id &&
                        d.ReintegroVenta.Estado == EstadoReintegro.Activo)
                    .GroupBy(d =>
                        d.ProductoId)
                    .Select(g => new
                    {
                        ProductoId =
                            g.Key,

                        Cantidad =
                            g.Sum(d =>
                                d.Cantidad)
                    })
                    .ToDictionaryAsync(
                        x => x.ProductoId,
                        x => x.Cantidad);

            var vm =
                new RegistrarReintegroVentaVM
                {
                    VentaId =
                        venta.Id,

                    ImporteDisponible =
                        importeDisponible,

                    Detalles =
                        venta.Detalles
                            .Select(d =>
                                new ReintegroVentaDetalleVM
                                {
                                    ProductoId =
                                        d.ProductoId,

                                    ProductoNombre =
                                        d.Producto.Nombre,

                                    CantidadVendida =
                                        d.Cantidad,

                                    CantidadYaReintegrada =
                                        cantidadesReintegradas
                                            .GetValueOrDefault(
                                                d.ProductoId),

                                    PrecioUnitario =
                                        d.PrecioUnitario,

                                    CantidadReintegrar =
                                        0
                                })
                            .Where(d =>
                                d.CantidadDisponible > 0)
                            .ToList()
                };

            if (!vm.Detalles.Any())
            {
                TempData["Error"] =
                    "Todos los productos de esta venta ya fueron reintegrados.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            await CargarOpciones(
                vm,
                venta.EmpresaId);

            return View(vm);
        }



        //Helper Methods
        private async Task CargarOpciones(RegistrarReintegroVentaVM vm, int empresaId)
        {
            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c =>
                        c.Nombre)
                    .Select(c =>
                        new CajaOpcionSimpleVM
                        {
                            Id = c.Id,
                            Nombre = c.Nombre
                        })
                    .ToListAsync();

            vm.MediosPagoDisponibles =
                await _context.MediosPago
                    .AsNoTracking()
                    .Where(m =>
                        m.EmpresaId == empresaId &&
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .Select(m =>
                        new MedioPagoOpcionSimpleVM
                        {
                            Id = m.Id,
                            Nombre = m.Nombre,
                            Tipo = m.Tipo
                        })
                    .ToListAsync();
        }
    }
}