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
    public class PagoProveedorController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly CompraSaldoService _compraSaldoService;
        private readonly CajaSaldoService _cajaSaldoService;

        public PagoProveedorController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            CompraSaldoService compraSaldoService,
            CajaSaldoService cajaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _compraSaldoService = compraSaldoService;
            _cajaSaldoService = cajaSaldoService;
        }

        // GET: PagoProveedor/Registrar/5
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
                    .AsNoTracking();

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
                    "No se pueden registrar pagos sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            decimal saldoPendiente =
                await _compraSaldoService
                    .ObtenerSaldoPendiente(
                        compra.Id,
                        compra.Total);

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La compra no tiene saldo pendiente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            var vm = new RegistrarPagoProveedorVM
            {
                CompraId =
                    compra.Id,

                SaldoPendiente =
                    saldoPendiente,

                Importe =
                    saldoPendiente
            };

            await CargarOpciones(
                vm,
                compra.EmpresaId);

            return View(vm);
        }
        // POST: PagoProveedor/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarPagoProveedorVM vm)
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
                    "No se pueden registrar pagos sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            decimal saldoPendiente =
                await _compraSaldoService
                    .ObtenerSaldoPendiente(
                        compra.Id,
                        compra.Total);

            vm.SaldoPendiente =
                saldoPendiente;

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La compra ya se encuentra completamente pagada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            if (vm.Importe > saldoPendiente)
            {
                ModelState.AddModelError(
                    nameof(vm.Importe),
                    "El importe no puede superar el saldo pendiente.");
            }

            var caja =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CajaId &&
                        c.EmpresaId == compra.EmpresaId &&
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
                            cm.Caja.EmpresaId == compra.EmpresaId &&
                            cm.Caja.Estado &&
                            cm.MedioPago.EmpresaId == compra.EmpresaId &&
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
                        t.EmpresaId == compra.EmpresaId &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

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
            }

            if (!ModelState.IsValid)
            {
                await CargarOpciones(
                    vm,
                    compra.EmpresaId);

                return View(vm);
            }

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
                        "La compra fue anulada antes de registrar el pago.";

                    return RedirectToAction(
                        "Details",
                        "Compra",
                        new { id = compra.Id });
                }

                decimal saldoPendienteActual =
                    await _compraSaldoService
                        .ObtenerSaldoPendiente(
                            compraActual.Id,
                            compraActual.Total);

                if (saldoPendienteActual <= 0)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] =
                        "La compra ya se encuentra completamente pagada.";

                    return RedirectToAction(
                        "Details",
                        "Compra",
                        new { id = compraActual.Id });
                }

                if (vm.Importe > saldoPendienteActual)
                {
                    await transaccion.RollbackAsync();

                    vm.SaldoPendiente =
                        saldoPendienteActual;

                    ModelState.AddModelError(
                        nameof(vm.Importe),
                        $"El saldo pendiente cambió. Saldo actual: {saldoPendienteActual:C}.");

                    await CargarOpciones(
                        vm,
                        compraActual.EmpresaId);

                    return View(vm);
                }

                var cajaActual =
                    await _context.Cajas
                        .FirstOrDefaultAsync(c =>
                            c.Id == vm.CajaId &&
                            c.EmpresaId == compraActual.EmpresaId &&
                            c.Estado);

                if (cajaActual == null)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        "La caja seleccionada dejó de estar disponible.");

                    await CargarOpciones(
                        vm,
                        compraActual.EmpresaId);

                    return View(vm);
                }

                bool medioPagoValidoActual =
                    await _context.CajaMediosPago
                        .AsNoTracking()
                        .AnyAsync(cm =>
                            cm.CajaId == vm.CajaId &&
                            cm.MedioPagoId == vm.MedioPagoId &&
                            cm.Caja.EmpresaId == compraActual.EmpresaId &&
                            cm.Caja.Estado &&
                            cm.MedioPago.EmpresaId == compraActual.EmpresaId &&
                            cm.MedioPago.Estado);

                if (!medioPagoValidoActual)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.MedioPagoId),
                        "El medio de pago ya no es válido para la caja seleccionada.");

                    await CargarOpciones(
                        vm,
                        compraActual.EmpresaId);

                    return View(vm);
                }

                var turnoOperativoActual =
                    await _context.TurnosCaja
                        .FirstOrDefaultAsync(t =>
                            t.EmpresaId == compraActual.EmpresaId &&
                            t.UsuarioAperturaId == usuario.Id &&
                            t.Estado == EstadoTurnoCaja.Abierto);

                int? turnoMovimientoCajaId = null;

                if (cajaActual.PermiteTurnos)
                {
                    if (turnoOperativoActual == null ||
                        turnoOperativoActual.CajaId != cajaActual.Id)
                    {
                        await transaccion.RollbackAsync();

                        ModelState.AddModelError(
                            nameof(vm.CajaId),
                            $"Debe tener un turno abierto propio para operar la caja \"{cajaActual.Nombre}\".");

                        await CargarOpciones(
                            vm,
                            compraActual.EmpresaId);

                        return View(vm);
                    }

                    turnoMovimientoCajaId =
                        turnoOperativoActual.Id;
                }

                decimal saldoDisponibleCaja =
                    await _cajaSaldoService
                        .CalcularSaldoDisponible(
                            cajaActual,
                            usuario.Id);

                if (vm.Importe > saldoDisponibleCaja)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.Importe),
                        $"La caja no tiene saldo suficiente. Disponible: {saldoDisponibleCaja:C}.");

                    await CargarOpciones(
                        vm,
                        compraActual.EmpresaId);

                    return View(vm);
                }

                var pago = new PagoProveedor
                {
                    CompraId =
                        compraActual.Id,

                    EmpresaId =
                        compraActual.EmpresaId,

                    CajaId =
                        vm.CajaId,

                    MedioPagoId =
                        vm.MedioPagoId,

                    TurnoCajaId =
                        turnoMovimientoCajaId,

                    UsuarioId =
                        usuario.Id,

                    Fecha =
                        DateTime.Now,

                    Importe =
                        vm.Importe,

                    Estado =
                        EstadoPago.Activo,

                    FechaAnulacion =
                        null,

                    UsuarioAnulacionId =
                        null,

                    MotivoAnulacion =
                        null
                };

                _context.PagosProveedor.Add(
                    pago);

                await _context.SaveChangesAsync();

                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            compraActual.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.PagoProveedor,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            pago.Fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turnoMovimientoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Pago de compra #{compraActual.Id}",

                        Observaciones =
                            null,

                        PagoProveedorId =
                            pago.Id
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    vm.Importe == saldoPendienteActual
                        ? "La compra quedó completamente pagada."
                        : "Pago registrado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compraActual.Id });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError("", "Ocurrió un error al registrar el pago al proveedor.");

                await CargarOpciones(
                    vm,
                    compra.EmpresaId);

                return View(vm);
            }
        }
        // GET: PagoProveedor/Anular/5
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

            IQueryable<PagoProveedor> consulta =
                _context.PagosProveedor
                    .AsNoTracking()
                    .Include(p => p.MedioPago)
                    .Include(p => p.Compra);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(p =>
                        p.EmpresaId == usuario.EmpresaId);
            }

            var pago =
                await consulta
                    .FirstOrDefaultAsync(p =>
                        p.Id == id);

            if (pago == null)
            {
                return NotFound();
            }

            if (pago.Estado != EstadoPago.Activo)
            {
                TempData["Error"] =
                    "El pago ya se encuentra anulado.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = pago.CompraId });
            }

            var movimiento =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.PagoProveedorId == pago.Id &&
                        m.Tipo == TipoMovimientoCaja.PagoProveedor);

            if (movimiento == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al pago.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = pago.CompraId });
            }

            bool yaRevertido =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.MovimientoOrigenId == movimiento.Id);

            if (yaRevertido)
            {
                TempData["Error"] =
                    "El movimiento asociado a este pago ya fue revertido.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = pago.CompraId });
            }

            var vm =
                new AnularPagoProveedorVM
                {
                    PagoProveedorId =
                        pago.Id,

                    CompraId =
                        pago.CompraId,

                    Importe =
                        pago.Importe,

                    MedioPagoNombre =
                        pago.MedioPago.Nombre
                };

            return View(vm);
        }
        // POST: PagoProveedor/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(AnularPagoProveedorVM vm)
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

            IQueryable<PagoProveedor> consulta =
                _context.PagosProveedor
                    .Include(p => p.MedioPago)
                    .Include(p => p.Compra);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(p =>
                        p.EmpresaId == usuario.EmpresaId);
            }

            var pago =
                await consulta
                    .FirstOrDefaultAsync(p =>
                        p.Id == vm.PagoProveedorId);

            if (pago == null)
            {
                return NotFound();
            }

            vm.CompraId =
                pago.CompraId;

            vm.Importe =
                pago.Importe;

            vm.MedioPagoNombre =
                pago.MedioPago.Nombre;

            if (pago.Estado != EstadoPago.Activo)
            {
                ModelState.AddModelError(
                    "",
                    "El pago ya se encuentra anulado.");
            }

            var movimiento =
                await _context.MovimientosCaja
                    .FirstOrDefaultAsync(m =>
                        m.PagoProveedorId == pago.Id &&
                        m.Tipo == TipoMovimientoCaja.PagoProveedor);

            if (movimiento == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró el movimiento de caja asociado al pago.");
            }

            if (movimiento != null)
            {
                bool yaRevertido =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.MovimientoOrigenId == movimiento.Id);

                if (yaRevertido)
                {
                    ModelState.AddModelError(
                        "",
                        "El movimiento asociado a este pago ya fue revertido.");
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
                var pagoActual =
                    await _context.PagosProveedor
                        .FirstOrDefaultAsync(p =>
                            p.Id == pago.Id &&
                            p.EmpresaId == pago.EmpresaId);

                if (pagoActual == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound();
                }

                if (pagoActual.Estado != EstadoPago.Activo)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "El pago ya fue anulado.");

                    return View(vm);
                }

                var movimientoActual =
                    await _context.MovimientosCaja
                        .FirstOrDefaultAsync(m =>
                            m.PagoProveedorId == pagoActual.Id &&
                            m.Tipo == TipoMovimientoCaja.PagoProveedor);

                if (movimientoActual == null)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "No se encontró el movimiento de caja asociado al pago.");

                    return View(vm);
                }

                bool movimientoYaRevertido =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.MovimientoOrigenId ==
                            movimientoActual.Id);

                if (movimientoYaRevertido)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "El movimiento asociado a este pago ya fue revertido.");

                    return View(vm);
                }

                pagoActual.Estado =
                    EstadoPago.Anulado;

                pagoActual.FechaAnulacion =
                    DateTime.Now;

                pagoActual.UsuarioAnulacionId =
                    usuario.Id;

                pagoActual.MotivoAnulacion =
                    vm.Motivo;

                var movimientoReversion =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            movimientoActual.EmpresaId,

                        CajaId =
                            movimientoActual.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReversionPagoProveedor,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            movimientoActual.Importe,

                        Fecha =
                            DateTime.Now,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            movimientoActual.MedioPagoId,

                        TurnoCajaId =
                            movimientoActual.TurnoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reversión de pago de compra #{pagoActual.CompraId}",

                        Observaciones =
                            vm.Motivo,

                        MovimientoOrigenId =
                            movimientoActual.Id,

                        PagoProveedorId =
                            pagoActual.Id
                    };

                _context.MovimientosCaja.Add(
                    movimientoReversion);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Pago anulado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = pagoActual.CompraId });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al anular el pago.");

                return View(vm);
            }
        }
        // GET: PagoProveedor/GetCajasPorMedioPago
        [HttpGet]
        public async Task<IActionResult> GetCajasPorMedioPago(int compraId, int medioPagoId)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<Compra> consultaCompra =
                _context.Compras
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consultaCompra =
                    consultaCompra.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
            }

            var compra =
                await consultaCompra
                    .FirstOrDefaultAsync(c =>
                        c.Id == compraId);

            if (compra == null)
            {
                return NotFound();
            }

            var cajas =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm =>
                        cm.MedioPagoId == medioPagoId &&
                        cm.MedioPago.EmpresaId == compra.EmpresaId &&
                        cm.MedioPago.Estado &&
                        cm.Caja.EmpresaId == compra.EmpresaId &&
                        cm.Caja.Estado)
                    .OrderBy(cm =>
                        cm.Caja.Nombre)
                    .Select(cm => new
                    {
                        id = cm.CajaId,
                        nombre = cm.Caja.Nombre
                    })
                    .ToListAsync();

            return Json(cajas);
        }

        //Helpers Methods
        private async Task CargarOpciones( RegistrarPagoProveedorVM vm, int empresaId)
        {
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