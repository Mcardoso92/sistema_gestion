using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ReintegroVentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly CajaSaldoService _cajaSaldoService;
        private readonly VentaSaldoService _ventaSaldoService;

        public ReintegroVentaController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            CajaSaldoService cajaSaldoService,
            VentaSaldoService ventaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _cajaSaldoService = cajaSaldoService;
            _ventaSaldoService = ventaSaldoService;
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

            decimal importeDisponible =
                await _ventaSaldoService
                    .ObtenerImporteDisponibleReintegro(
                        venta.Id);

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
        // POST: ReintegroVenta/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarReintegroVentaVM vm)
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

            IQueryable<Venta> consultaVenta =
                _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consultaVenta =
                    consultaVenta.Where(v =>
                        v.EmpresaId == usuario.EmpresaId);
            }

            var venta =
                await consultaVenta
                    .FirstOrDefaultAsync(v =>
                        v.Id == vm.VentaId);

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

            decimal importeDisponible =
                await _ventaSaldoService
                    .ObtenerImporteDisponibleReintegro(
                        venta.Id);

            vm.ImporteDisponible =
                importeDisponible;

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
                        d.ReintegroVenta.Estado ==
                            EstadoReintegro.Activo)
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

            var detallesSolicitados =
                vm.Detalles
                    .Where(d =>
                        d.CantidadReintegrar > 0)
                    .ToList();

            if (!detallesSolicitados.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Debe seleccionar al menos un producto para reintegrar.");
            }

            decimal importeReintegro = 0;

            var detallesValidados =
                new List<DetalleReintegroVenta>();

            foreach (var solicitado in detallesSolicitados)
            {
                var detalleVenta =
                    venta.Detalles
                        .FirstOrDefault(d =>
                            d.ProductoId ==
                            solicitado.ProductoId);

                if (detalleVenta == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Uno de los productos seleccionados no pertenece a la venta.");

                    continue;
                }

                int cantidadYaReintegrada =
                    cantidadesReintegradas
                        .GetValueOrDefault(
                            detalleVenta.ProductoId);

                int cantidadDisponible =
                    Math.Max(
                        0,
                        detalleVenta.Cantidad -
                        cantidadYaReintegrada);

                if (solicitado.CantidadReintegrar >
                    cantidadDisponible)
                {
                    ModelState.AddModelError(
                        "",
                        $"La cantidad a reintegrar de \"{detalleVenta.Producto.Nombre}\" supera la cantidad disponible.");

                    continue;
                }

                decimal subtotal =
                    detalleVenta.PrecioUnitario *
                    solicitado.CantidadReintegrar;

                importeReintegro +=
                    subtotal;

                detallesValidados.Add(
                    new DetalleReintegroVenta
                    {
                        ProductoId =
                            detalleVenta.ProductoId,

                        Cantidad =
                            solicitado.CantidadReintegrar,

                        PrecioUnitario =
                            detalleVenta.PrecioUnitario,

                        Subtotal =
                            subtotal
                    });
            }

            if (importeReintegro <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "El importe del reintegro debe ser mayor a cero.");
            }

            if (importeReintegro >
                importeDisponible)
            {
                ModelState.AddModelError(
                    "",
                    "El importe del reintegro supera el importe disponible de la venta.");
            }

            var caja =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CajaId &&
                        c.EmpresaId == venta.EmpresaId &&
                        c.Estado);

            if (caja == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no es válida.");
            }

            bool medioPagoValido = false;

            if (caja != null)
            {
                medioPagoValido =
                    await _context.CajaMediosPago
                        .AsNoTracking()
                        .AnyAsync(cm =>
                            cm.CajaId == vm.CajaId &&
                            cm.MedioPagoId == vm.MedioPagoId &&
                            cm.Caja.EmpresaId ==
                                venta.EmpresaId &&
                            cm.Caja.Estado &&
                            cm.MedioPago.EmpresaId ==
                                venta.EmpresaId &&
                            cm.MedioPago.Estado);
            }

            if (!medioPagoValido)
            {
                ModelState.AddModelError(
                    nameof(vm.MedioPagoId),
                    "El medio de pago no es válido para la caja seleccionada.");
            }

            var turnoOperativo =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.EmpresaId == venta.EmpresaId &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

            int? turnoMovimientoCajaId = null;

            if (caja != null &&
                caja.PermiteTurnos)
            {
                if (turnoOperativo == null ||
                    turnoOperativo.CajaId != caja.Id)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        $"Debe tener un turno abierto propio para operar la caja \"{caja.Nombre}\".");
                }
                else
                {
                    turnoMovimientoCajaId =
                        turnoOperativo.Id;
                }
            }

            decimal saldoDisponibleCaja = 0;

            if (caja != null)
            {
                saldoDisponibleCaja =
                    await _cajaSaldoService
                        .CalcularSaldoDisponible(
                            caja,
                            usuario.Id);
            }

            if (importeReintegro >
                saldoDisponibleCaja)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    $"La caja no tiene saldo suficiente para realizar el reintegro. Saldo disponible: {saldoDisponibleCaja:C}.");
            }

            if (!ModelState.IsValid)
            {
                await ReconstruirVM(
                    vm,
                    venta);

                return View(vm);
            }

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                // Revalidación final dentro de la transacción SERIALIZABLE.
                // No confiamos en las validaciones realizadas antes de abrirla.

                decimal importeDisponibleActual =
                    await _ventaSaldoService
                        .ObtenerImporteDisponibleReintegro(
                            venta.Id);

                if (importeReintegro >
                    importeDisponibleActual)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "El importe disponible para reintegrar cambió. Actualice la operación e inténtelo nuevamente.");

                    await ReconstruirVM(
                        vm,
                        venta);

                    return View(vm);
                }

                var cantidadesReintegradasActuales =
                    await _context.DetallesReintegroVenta
                        .Where(d =>
                            d.ReintegroVenta.VentaId ==
                                venta.Id &&
                            d.ReintegroVenta.Estado ==
                                EstadoReintegro.Activo)
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

                foreach (var detalle in detallesValidados)
                {
                    var detalleVenta =
                        venta.Detalles
                            .First(d =>
                                d.ProductoId ==
                                detalle.ProductoId);

                    int yaReintegrado =
                        cantidadesReintegradasActuales
                            .GetValueOrDefault(
                                detalle.ProductoId);

                    int disponibleActual =
                        Math.Max(
                            0,
                            detalleVenta.Cantidad -
                            yaReintegrado);

                    if (detalle.Cantidad >
                        disponibleActual)
                    {
                        await transaccion.RollbackAsync();

                        ModelState.AddModelError(
                            "",
                            $"La cantidad disponible de \"{detalleVenta.Producto.Nombre}\" cambió. Disponible actualmente: {disponibleActual}.");

                        await ReconstruirVM(
                            vm,
                            venta);

                        return View(vm);
                    }
                }

                decimal saldoCajaActual =
                    await _cajaSaldoService
                        .CalcularSaldoDisponible(
                            caja!,
                            usuario.Id);

                if (importeReintegro >
                    saldoCajaActual)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        $"El saldo disponible de la caja cambió. Saldo actual: {saldoCajaActual:C}.");

                    await ReconstruirVM(
                        vm,
                        venta);

                    return View(vm);
                }

                var fecha =
                    DateTime.Now;

                var reintegro =
                    new ReintegroVenta
                    {
                        VentaId =
                            venta.Id,

                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turnoOperativo?.Id,

                        UsuarioId =
                            usuario.Id,

                        Fecha =
                            fecha,

                        Importe =
                            importeReintegro,

                        Estado =
                            EstadoReintegro.Activo,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                _context.ReintegrosVenta.Add(
                    reintegro);

                await _context.SaveChangesAsync();

                foreach (var detalle in detallesValidados)
                {
                    detalle.ReintegroVentaId =
                        reintegro.Id;

                    _context.DetallesReintegroVenta.Add(
                        detalle);

                    var producto =
                        venta.Detalles
                            .First(d =>
                                d.ProductoId ==
                                detalle.ProductoId)
                            .Producto;

                    int stockAnterior =
                        producto.Stock;

                    producto.Stock +=
                        detalle.Cantidad;

                    var movimientoStock =
                        new MovimientoStock
                        {
                            ProductoId =
                                producto.Id,

                            EmpresaId =
                                venta.EmpresaId,

                            Tipo =
                                TipoMovimientoStock.ReintegroVenta,

                            Cantidad =
                                detalle.Cantidad,

                            StockAnterior =
                                stockAnterior,

                            StockPosterior =
                                producto.Stock,

                            Motivo =
                                $"Reintegro de venta #{venta.Id}",

                            Fecha =
                                fecha,

                            UsuarioId =
                                usuario.Id,

                            VentaId =
                                venta.Id,

                            CompraId =
                                null,

                            ReintegroVentaId =
                                reintegro.Id
                        };

                    _context.MovimientosStock.Add(
                        movimientoStock);
                }

                var movimientoCaja =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReintegroVenta,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            importeReintegro,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turnoMovimientoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reintegro de venta #{venta.Id}",

                        Observaciones =
                            null,

                        ReintegroVentaId =
                            reintegro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimientoCaja);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Reintegro registrado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el reintegro.");

                await ReconstruirVM(
                    vm,
                    venta);

                return View(vm);
            }
        }
        // GET: ReintegroVenta/Anular/5
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

            IQueryable<ReintegroVenta> consulta =
                _context.ReintegrosVenta
                    .AsNoTracking()
                    .Include(r => r.MedioPago)
                    .Include(r => r.Venta);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(r =>
                        r.EmpresaId == usuario.EmpresaId);
            }

            var reintegro =
                await consulta
                    .FirstOrDefaultAsync(r =>
                        r.Id == id);

            if (reintegro == null)
            {
                return NotFound();
            }

            if (reintegro.Estado != EstadoReintegro.Activo)
            {
                TempData["Error"] =
                    "El reintegro ya se encuentra anulado.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = reintegro.VentaId });
            }

            var movimientoCaja =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.ReintegroVentaId == reintegro.Id &&
                        m.Tipo == TipoMovimientoCaja.ReintegroVenta);

            if (movimientoCaja == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al reintegro.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = reintegro.VentaId });
            }

            bool yaRevertido =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.MovimientoOrigenId ==
                            movimientoCaja.Id);

            if (yaRevertido)
            {
                TempData["Error"] =
                    "El movimiento asociado a este reintegro ya fue revertido.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = reintegro.VentaId });
            }

            var vm =
                new AnularReintegroVentaVM
                {
                    ReintegroVentaId =
                        reintegro.Id,

                    VentaId =
                        reintegro.VentaId,

                    Importe =
                        reintegro.Importe,

                    MedioPagoNombre =
                        reintegro.MedioPago.Nombre
                };

            return View(vm);
        }
        // POST: ReintegroVenta/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(AnularReintegroVentaVM vm)
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

            IQueryable<ReintegroVenta> consulta =
                _context.ReintegrosVenta
                    .Include(r => r.MedioPago)
                    .Include(r => r.Venta)
                    .Include(r => r.Detalles)
                        .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(r =>
                        r.EmpresaId == usuario.EmpresaId);
            }

            var reintegro =
                await consulta
                    .FirstOrDefaultAsync(r =>
                        r.Id == vm.ReintegroVentaId);

            if (reintegro == null)
            {
                return NotFound();
            }

            vm.VentaId =
                reintegro.VentaId;

            vm.Importe =
                reintegro.Importe;

            vm.MedioPagoNombre =
                reintegro.MedioPago.Nombre;

            if (reintegro.Estado != EstadoReintegro.Activo)
            {
                ModelState.AddModelError(
                    "",
                    "El reintegro ya se encuentra anulado.");
            }

            var movimientoCaja =
                await _context.MovimientosCaja
                    .FirstOrDefaultAsync(m =>
                        m.ReintegroVentaId == reintegro.Id &&
                        m.Tipo == TipoMovimientoCaja.ReintegroVenta);

            if (movimientoCaja == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró el movimiento de caja asociado al reintegro.");
            }

            if (movimientoCaja != null)
            {
                bool yaRevertido =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.MovimientoOrigenId ==
                                movimientoCaja.Id);

                if (yaRevertido)
                {
                    ModelState.AddModelError(
                        "",
                        "El movimiento asociado al reintegro ya fue revertido.");
                }
            }

            foreach (var detalle in reintegro.Detalles)
            {
                if (detalle.Producto.Stock <
                    detalle.Cantidad)
                {
                    ModelState.AddModelError(
                        "",
                        $"No se puede anular el reintegro porque el stock actual de \"{detalle.Producto.Nombre}\" es insuficiente. " +
                        $"Stock actual: {detalle.Producto.Stock}. Cantidad a descontar: {detalle.Cantidad}.");
                }
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
                // Revalidamos stock dentro de la transacción.
                foreach (var detalle in reintegro.Detalles)
                {
                    if (detalle.Producto.Stock <
                        detalle.Cantidad)
                    {
                        await transaccion.RollbackAsync();

                        ModelState.AddModelError(
                            "",
                            $"El stock de \"{detalle.Producto.Nombre}\" cambió y ya no permite anular el reintegro.");

                        return View(vm);
                    }
                }                

                var fecha =
                    DateTime.Now;

                reintegro.Estado =
                    EstadoReintegro.Anulado;

                reintegro.FechaAnulacion =
                    fecha;

                reintegro.UsuarioAnulacionId =
                    usuario.Id;

                reintegro.MotivoAnulacion =
                    vm.Motivo;

                foreach (var detalle in reintegro.Detalles)
                {
                    int stockAnterior =
                        detalle.Producto.Stock;

                    int stockPosterior =
                        stockAnterior -
                        detalle.Cantidad;

                    detalle.Producto.Stock =
                        stockPosterior;

                    _context.MovimientosStock.Add(
                        new MovimientoStock
                        {
                            ProductoId =
                                detalle.ProductoId,

                            EmpresaId =
                                reintegro.EmpresaId,

                            Tipo =
                                TipoMovimientoStock.AnulacionReintegroVenta,

                            Cantidad =
                                detalle.Cantidad,

                            StockAnterior =
                                stockAnterior,

                            StockPosterior =
                                stockPosterior,

                            Motivo =
                                $"Anulación reintegro #{reintegro.Id} - Venta #{reintegro.VentaId}",

                            Fecha =
                                fecha,

                            UsuarioId =
                                usuario.Id,

                            VentaId =
                                reintegro.VentaId,

                            CompraId =
                                null,

                            ReintegroVentaId =
                                reintegro.Id
                        });
                }

                var movimientoReversion =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            movimientoCaja!.EmpresaId,

                        CajaId =
                            movimientoCaja.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReversionReintegroVenta,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            movimientoCaja.Importe,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            movimientoCaja.MedioPagoId,

                        TurnoCajaId =
                            movimientoCaja.TurnoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reversión reintegro #{reintegro.Id} - Venta #{reintegro.VentaId}",

                        Observaciones =
                            vm.Motivo,

                        MovimientoOrigenId =
                            movimientoCaja.Id,

                        ReintegroVentaId =
                            reintegro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimientoReversion);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Reintegro anulado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = reintegro.VentaId });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al anular el reintegro.");

                return View(vm);
            }
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
        private async Task ReconstruirVM(RegistrarReintegroVentaVM vm, Venta venta)
        {
            vm.ImporteDisponible =
                await _ventaSaldoService
                    .ObtenerImporteDisponibleReintegro(
                        venta.Id);

            var cantidadesReintegradas =
                await _context.DetallesReintegroVenta
                    .AsNoTracking()
                    .Where(d =>
                        d.ReintegroVenta.VentaId == venta.Id &&
                        d.ReintegroVenta.Estado ==
                            EstadoReintegro.Activo)
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

            var cantidadesSolicitadas =
                vm.Detalles
                    .ToDictionary(
                        d => d.ProductoId,
                        d => d.CantidadReintegrar);

            vm.Detalles =
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
                                cantidadesSolicitadas
                                    .GetValueOrDefault(
                                        d.ProductoId)
                        })
                    .Where(d =>
                        d.CantidadDisponible > 0)
                    .ToList();

            await CargarOpciones(
                vm,
                venta.EmpresaId);
        }
    }
}