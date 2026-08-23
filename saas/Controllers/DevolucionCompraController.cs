using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel.DevolucionCompra;
using saas.ViewModels.DevolucionCompra;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class DevolucionCompraController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public DevolucionCompraController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: DevolucionCompra/Registrar?compraId=5
        [HttpGet]
        public async Task<IActionResult> Registrar(int compraId)
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

            IQueryable<Compra> consulta =
                _context.Compras
                    .AsNoTracking()
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
            }

            var compra =
                await consulta
                    .FirstOrDefaultAsync(c =>
                        c.Id == compraId);

            if (compra == null)
            {
                return NotFound();
            }

            if (!compra.Estado)
            {
                TempData["Error"] =
                    "No se pueden registrar devoluciones sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            var cantidadesDevueltas =
                await _context.DetallesDevolucionCompra
                    .AsNoTracking()
                    .Where(d =>
                        d.DevolucionCompra.CompraId == compra.Id &&
                        d.DevolucionCompra.EmpresaId == compra.EmpresaId &&
                        d.DevolucionCompra.Estado)
                    .GroupBy(d =>
                        d.DetalleCompraId)
                    .Select(g => new
                    {
                        DetalleCompraId =
                            g.Key,

                        CantidadDevuelta =
                            g.Sum(d =>
                                d.Cantidad)
                    })
                    .ToDictionaryAsync(
                        x => x.DetalleCompraId,
                        x => x.CantidadDevuelta);

            var vm =
                new RegistrarDevolucionCompraVM
                {
                    CompraId =
                        compra.Id,

                    ProveedorNombre =
                        compra.Proveedor.RazonSocial,

                    FechaCompra =
                        compra.Fecha,

                    TotalCompra =
                        compra.Total,

                    Detalles =
                        compra.Detalles
                            .OrderBy(d =>
                                d.Producto.Nombre)
                            .Select(d =>
                            {
                                cantidadesDevueltas.TryGetValue(
                                    d.Id,
                                    out int cantidadDevuelta);

                                int cantidadDisponible =
                                    Math.Max(
                                        0,
                                        d.Cantidad - cantidadDevuelta);

                                return new RegistrarDetalleDevolucionCompraVM
                                {
                                    DetalleCompraId =
                                        d.Id,

                                    ProductoId =
                                        d.ProductoId,

                                    ProductoNombre =
                                        d.Producto.Nombre,

                                    CantidadComprada =
                                        d.Cantidad,

                                    CantidadDevuelta =
                                        cantidadDevuelta,

                                    CantidadDisponible =
                                        cantidadDisponible,

                                    PrecioUnitario =
                                        d.PrecioUnitario,

                                    CantidadDevolver =
                                        0
                                };
                            })
                            .ToList()
                };

            if (!vm.Detalles.Any(d =>
                    d.CantidadDisponible > 0))
            {
                TempData["Error"] =
                    "No quedan productos disponibles para devolver en esta compra.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            return View(vm);
        }
        // POST: DevolucionCompra/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarDevolucionCompraVM vm)
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

            IQueryable<Compra> consultaCompra =
                _context.Compras;

            if (!esSuperAdmin)
            {
                consultaCompra =
                    consultaCompra.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
            }

            var compra =
                await consultaCompra
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CompraId);

            if (compra == null)
            {
                return NotFound();
            }

            if (!compra.Estado)
            {
                TempData["Error"] =
                    "No se pueden registrar devoluciones sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            if (vm.Detalles == null ||
                !vm.Detalles.Any(d =>
                    d.CantidadDevolver > 0))
            {
                ModelState.AddModelError(
                    nameof(vm.Detalles),
                    "Debe indicar al menos un producto para devolver.");
            }

            if (!ModelState.IsValid)
            {
                return await PrepararVistaRegistrar(
                    vm,
                    compra);
            }

            vm.Observaciones =
                string.IsNullOrWhiteSpace(vm.Observaciones)
                    ? null
                    : vm.Observaciones.Trim();

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var compraActual =
                    await _context.Compras
                        .FirstOrDefaultAsync(c =>
                            c.Id == compra.Id &&
                            c.EmpresaId == compra.EmpresaId);

                if (compraActual == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound();
                }

                if (!compraActual.Estado)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] =
                        "La compra fue anulada antes de registrar la devolución.";

                    return RedirectToAction(
                        "Details",
                        "Compra",
                        new { id = compraActual.Id });
                }

                var detallesSolicitados =
                    vm.Detalles
                        .Where(d =>
                            d.CantidadDevolver > 0)
                        .ToList();

                var detallesIds =
                    detallesSolicitados
                        .Select(d =>
                            d.DetalleCompraId)
                        .Distinct()
                        .ToList();

                if (detallesIds.Count !=
                    detallesSolicitados.Count)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.Detalles),
                        "La devolución contiene detalles duplicados.");

                    return await PrepararVistaRegistrar(
                        vm,
                        compraActual);
                }

                var detallesCompra =
                    await _context.DetallesCompra
                        .Include(d =>
                            d.Producto)
                        .Where(d =>
                            detallesIds.Contains(d.Id) &&
                            d.CompraId == compraActual.Id)
                        .ToListAsync();

                if (detallesCompra.Count !=
                    detallesIds.Count)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.Detalles),
                        "Uno o más productos no pertenecen a la compra.");

                    return await PrepararVistaRegistrar(
                        vm,
                        compraActual);
                }

                var detallesPorId =
                    detallesCompra
                        .ToDictionary(d =>
                            d.Id);

                var cantidadesDevueltas =
                    await _context.DetallesDevolucionCompra
                        .AsNoTracking()
                        .Where(d =>
                            detallesIds.Contains(
                                d.DetalleCompraId) &&
                            d.DevolucionCompra.CompraId ==
                                compraActual.Id &&
                            d.DevolucionCompra.EmpresaId ==
                                compraActual.EmpresaId &&
                            d.DevolucionCompra.Estado)
                        .GroupBy(d =>
                            d.DetalleCompraId)
                        .Select(g => new
                        {
                            DetalleCompraId =
                                g.Key,

                            Cantidad =
                                g.Sum(d =>
                                    d.Cantidad)
                        })
                        .ToDictionaryAsync(
                            x =>
                                x.DetalleCompraId,
                            x =>
                                x.Cantidad);

                decimal totalDevolucion = 0;

                DateTime fechaDevolucion =
                    DateTime.Now;

                var devolucion =
                    new DevolucionCompra
                    {
                        CompraId =
                            compraActual.Id,

                        EmpresaId =
                            compraActual.EmpresaId,

                        UsuarioId =
                            usuario.Id,

                        Fecha =
                            fechaDevolucion,

                        Total =
                            0,

                        Estado =
                            true,

                        Observaciones =
                            vm.Observaciones,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                foreach (var detalleVM
                    in detallesSolicitados)
                {
                    var detalleCompra =
                        detallesPorId[
                            detalleVM.DetalleCompraId];

                    cantidadesDevueltas.TryGetValue(
                        detalleCompra.Id,
                        out int cantidadYaDevuelta);

                    int cantidadDisponible =
                        detalleCompra.Cantidad -
                        cantidadYaDevuelta;

                    if (detalleVM.CantidadDevolver >
                        cantidadDisponible)
                    {
                        await transaccion.RollbackAsync();

                        ModelState.AddModelError(
                            nameof(vm.Detalles),
                            $"La cantidad a devolver de \"{detalleCompra.Producto.Nombre}\" supera la cantidad disponible.");

                        return await PrepararVistaRegistrar(
                            vm,
                            compraActual);
                    }

                    if (detalleVM.CantidadDevolver >
                        detalleCompra.Producto.Stock)
                    {
                        await transaccion.RollbackAsync();

                        ModelState.AddModelError(
                            nameof(vm.Detalles),
                            $"No hay stock suficiente de \"{detalleCompra.Producto.Nombre}\" para realizar la devolución.");

                        return await PrepararVistaRegistrar(
                            vm,
                            compraActual);
                    }

                    decimal subtotal =
                        detalleVM.CantidadDevolver *
                        detalleCompra.PrecioUnitario;

                    devolucion.Detalles.Add(
                        new DetalleDevolucionCompra
                        {
                            DetalleCompraId =
                                detalleCompra.Id,

                            ProductoId =
                                detalleCompra.ProductoId,

                            Cantidad =
                                detalleVM.CantidadDevolver,

                            PrecioUnitario =
                                detalleCompra.PrecioUnitario,

                            Subtotal =
                                subtotal
                        });

                    int stockAnterior =
                        detalleCompra.Producto.Stock;

                    int stockPosterior =
                        stockAnterior -
                        detalleVM.CantidadDevolver;

                    detalleCompra.Producto.Stock =
                        stockPosterior;

                    devolucion.MovimientosStock.Add(
                        new MovimientoStock
                        {
                            ProductoId =
                                detalleCompra.ProductoId,

                            EmpresaId =
                                compraActual.EmpresaId,

                            Tipo =
                                TipoMovimientoStock.DevolucionCompra,

                            Cantidad =
                                detalleVM.CantidadDevolver,

                            StockAnterior =
                                stockAnterior,

                            StockPosterior =
                                stockPosterior,

                            Motivo =
                                vm.Observaciones,

                            Fecha =
                                fechaDevolucion,

                            UsuarioId =
                                usuario.Id
                        });

                    totalDevolucion +=
                        subtotal;
                }

                devolucion.Total =
                    totalDevolucion;

                _context.DevolucionesCompra.Add(
                    devolucion);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Devolución registrada correctamente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compraActual.Id });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar la devolución.");

                return await PrepararVistaRegistrar(
                    vm,
                    compra);
            }
        }
        // GET: DevolucionCompra/Anular/5
        [HttpGet]
        public async Task<IActionResult> Anular(int id)
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

            IQueryable<DevolucionCompra> consulta =
                _context.DevolucionesCompra
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(d =>
                        d.EmpresaId == usuario.EmpresaId);
            }

            var devolucion =
                await consulta
                    .FirstOrDefaultAsync(d =>
                        d.Id == id);

            if (devolucion == null)
            {
                return NotFound();
            }

            if (!devolucion.Estado)
            {
                TempData["Error"] =
                    "La devolución ya se encuentra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = devolucion.CompraId });
            }

            var vm =
                new AnularDevolucionCompraVM
                {
                    DevolucionCompraId =
                        devolucion.Id,

                    CompraId =
                        devolucion.CompraId,

                    Total =
                        devolucion.Total
                };

            return View(vm);
        }
        // POST: DevolucionCompra/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(AnularDevolucionCompraVM vm)
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

            IQueryable<DevolucionCompra> consulta =
                _context.DevolucionesCompra
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(d =>
                        d.EmpresaId == usuario.EmpresaId);
            }

            var devolucion =
                await consulta
                    .FirstOrDefaultAsync(d =>
                        d.Id == vm.DevolucionCompraId);

            if (devolucion == null)
            {
                return NotFound();
            }

            vm.CompraId =
                devolucion.CompraId;

            vm.Total =
                devolucion.Total;

            if (!devolucion.Estado)
            {
                ModelState.AddModelError(
                    "",
                    "La devolución ya se encuentra anulada.");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            vm.Motivo =
                vm.Motivo.Trim();

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var devolucionActual =
                    await _context.DevolucionesCompra
                        .Include(d => d.Detalles)
                            .ThenInclude(d => d.Producto)
                        .FirstOrDefaultAsync(d =>
                            d.Id == devolucion.Id &&
                            d.EmpresaId == devolucion.EmpresaId);

                if (devolucionActual == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound();
                }

                if (!devolucionActual.Estado)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "La devolución ya fue anulada.");

                    return View(vm);
                }

                decimal totalPagado =
    await _context.PagosProveedor
        .AsNoTracking()
        .Where(p =>
            p.CompraId == devolucionActual.CompraId &&
            p.EmpresaId == devolucionActual.EmpresaId &&
            p.Estado == EstadoPago.Activo)
        .SumAsync(p =>
            (decimal?)p.Importe)
    ?? 0;

                decimal totalReintegrado =
                    await _context.ReintegrosProveedor
                        .AsNoTracking()
                        .Where(r =>
                            r.CompraId == devolucionActual.CompraId &&
                            r.EmpresaId == devolucionActual.EmpresaId &&
                            r.Estado == EstadoReintegro.Activo)
                        .SumAsync(r =>
                            (decimal?)r.Importe)
                    ?? 0;

                decimal totalDevolucionesActivas =
                    await _context.DevolucionesCompra
                        .AsNoTracking()
                        .Where(d =>
                            d.CompraId == devolucionActual.CompraId &&
                            d.EmpresaId == devolucionActual.EmpresaId &&
                            d.Estado)
                        .SumAsync(d =>
                            (decimal?)d.Total)
                    ?? 0;

                decimal totalDevolucionesLuegoDeAnular =
                    totalDevolucionesActivas -
                    devolucionActual.Total;

                var compra =
                    await _context.Compras
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c =>
                            c.Id == devolucionActual.CompraId &&
                            c.EmpresaId == devolucionActual.EmpresaId);

                if (compra == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound();
                }

                decimal totalNetoLuegoDeAnular =
                    Math.Max(
                        0,
                        compra.Total -
                        totalDevolucionesLuegoDeAnular);

                decimal maximoReintegrableLuegoDeAnular =
                    Math.Max(
                        0,
                        totalPagado -
                        totalNetoLuegoDeAnular);

                if (totalReintegrado >
                    maximoReintegrableLuegoDeAnular)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "No se puede anular la devolución porque existen reintegros activos del proveedor que dejarían de estar justificados. Debe anular primero los reintegros correspondientes.");

                    return View(vm);
                }

                DateTime fechaAnulacion =
                    DateTime.Now;

                foreach (var detalle
                    in devolucionActual.Detalles)
                {
                    int stockAnterior =
                        detalle.Producto.Stock;

                    int stockPosterior =
                        stockAnterior +
                        detalle.Cantidad;

                    detalle.Producto.Stock =
                        stockPosterior;

                    devolucionActual.MovimientosStock.Add(
                        new MovimientoStock
                        {
                            ProductoId =
                                detalle.ProductoId,

                            EmpresaId =
                                devolucionActual.EmpresaId,

                            Tipo =
                                TipoMovimientoStock
                                    .AnulacionDevolucionCompra,

                            Cantidad =
                                detalle.Cantidad,

                            StockAnterior =
                                stockAnterior,

                            StockPosterior =
                                stockPosterior,

                            Motivo =
                                vm.Motivo,

                            Fecha =
                                fechaAnulacion,

                            UsuarioId =
                                usuario.Id
                        });
                }

                devolucionActual.Estado =
                    false;

                devolucionActual.FechaAnulacion =
                    fechaAnulacion;

                devolucionActual.UsuarioAnulacionId =
                    usuario.Id;

                devolucionActual.MotivoAnulacion =
                    vm.Motivo;

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Devolución anulada correctamente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new
                    {
                        id =
                            devolucionActual.CompraId
                    });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al anular la devolución.");

                return View(vm);
            }
        }

        //Helpers Methods
        private async Task<IActionResult> PrepararVistaRegistrar(RegistrarDevolucionCompraVM vm, Compra compra)
        {
            var compraCompleta =
                await _context.Compras
                    .AsNoTracking()
                    .Include(c => c.Proveedor)
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(c =>
                        c.Id == compra.Id &&
                        c.EmpresaId == compra.EmpresaId);

            if (compraCompleta == null)
            {
                return NotFound();
            }

            var cantidadesDevueltas =
                await _context.DetallesDevolucionCompra
                    .AsNoTracking()
                    .Where(d =>
                        d.DevolucionCompra.CompraId == compraCompleta.Id &&
                        d.DevolucionCompra.EmpresaId == compraCompleta.EmpresaId &&
                        d.DevolucionCompra.Estado)
                    .GroupBy(d =>
                        d.DetalleCompraId)
                    .Select(g => new
                    {
                        DetalleCompraId =
                            g.Key,

                        CantidadDevuelta =
                            g.Sum(d =>
                                d.Cantidad)
                    })
                    .ToDictionaryAsync(
                        x => x.DetalleCompraId,
                        x => x.CantidadDevuelta);

            var cantidadesSolicitadas =
                vm.Detalles?
                    .GroupBy(d =>
                        d.DetalleCompraId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().CantidadDevolver)
                ?? new Dictionary<int, int>();

            vm.CompraId =
                compraCompleta.Id;

            vm.ProveedorNombre =
                compraCompleta.Proveedor.RazonSocial;

            vm.FechaCompra =
                compraCompleta.Fecha;

            vm.TotalCompra =
                compraCompleta.Total;

            vm.Detalles =
                compraCompleta.Detalles
                    .OrderBy(d =>
                        d.Producto.Nombre)
                    .Select(d =>
                    {
                        cantidadesDevueltas.TryGetValue(
                            d.Id,
                            out int cantidadDevuelta);

                        cantidadesSolicitadas.TryGetValue(
                            d.Id,
                            out int cantidadSolicitada);

                        int cantidadDisponible =
                            Math.Max(
                                0,
                                d.Cantidad - cantidadDevuelta);

                        return new RegistrarDetalleDevolucionCompraVM
                        {
                            DetalleCompraId =
                                d.Id,

                            ProductoId =
                                d.ProductoId,

                            ProductoNombre =
                                d.Producto.Nombre,

                            CantidadComprada =
                                d.Cantidad,

                            CantidadDevuelta =
                                cantidadDevuelta,

                            CantidadDisponible =
                                cantidadDisponible,

                            PrecioUnitario =
                                d.PrecioUnitario,

                            CantidadDevolver =
                                cantidadSolicitada
                        };
                    })
                    .ToList();

            return View(
                "Registrar",
                vm);
        }
    }
}
