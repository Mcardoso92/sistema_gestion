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

        public ReintegroVentaController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            CajaSaldoService cajaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _cajaSaldoService = cajaSaldoService;
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
        // POST: ReintegroVenta/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarReintegroVentaVM vm)
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
        private async Task ReconstruirVM(
    RegistrarReintegroVentaVM vm,
    Venta venta)
        {
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

            vm.ImporteDisponible =
                Math.Max(
                    0,
                    totalCobrado - totalReintegrado);

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