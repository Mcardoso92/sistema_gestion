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
                await CargarOpciones(vm, compra.EmpresaId);

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

                    var pago =
    new PagoProveedor
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
            turnoOperativoActual?.Id,

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

            return View(vm);
        }
        private async Task CargarOpciones( RegistrarPagoProveedorVM vm, int empresaId)
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