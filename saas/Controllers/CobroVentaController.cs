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
    public class CobroVentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly VentaSaldoService _ventaSaldoService;
        public CobroVentaController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            VentaSaldoService ventaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _ventaSaldoService = ventaSaldoService;
        }

        // GET: CobroVenta/Registrar/5
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
                    .AsNoTracking();

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
                    "No se pueden registrar cobros sobre una venta anulada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            decimal saldoPendiente =
                await _ventaSaldoService
                    .ObtenerSaldoPendiente(
                        venta.Id,
                        venta.Total);

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La venta no tiene saldo pendiente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            var vm =
                new RegistrarCobroVentaVM
                {
                    VentaId =
                        venta.Id,

                    SaldoPendiente =
                        saldoPendiente,

                    Importe =
                        saldoPendiente
                };

            await CargarOpciones(
                vm,
                venta.EmpresaId);

            return View(vm);
        }
        // POST: CobroVenta/Registrar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarCobroVentaVM vm)
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
                _context.Ventas;

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
                    "No se pueden registrar cobros sobre una venta anulada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            decimal saldoPendiente =
                await _ventaSaldoService
                    .ObtenerSaldoPendiente(
                        venta.Id,
                        venta.Total);

            vm.SaldoPendiente =
                saldoPendiente;

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La venta ya se encuentra completamente cobrada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
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
                            cm.Caja.EmpresaId == venta.EmpresaId &&
                            cm.Caja.Estado &&
                            cm.MedioPago.EmpresaId == venta.EmpresaId &&
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

            if (!ModelState.IsValid)
            {
                await CargarOpciones(
                    vm,
                    venta.EmpresaId);

                return View(vm);
            }

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                decimal saldoPendienteActual =
                    await _ventaSaldoService
                        .ObtenerSaldoPendiente(
                            venta.Id,
                            venta.Total);

                if (saldoPendienteActual <= 0)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] =
                        "La venta ya se encuentra completamente cobrada.";

                    return RedirectToAction(
                        "Details",
                        "Venta",
                        new { id = venta.Id });
                }

                if (vm.Importe >
                    saldoPendienteActual)
                {
                    await transaccion.RollbackAsync();

                    vm.SaldoPendiente =
                        saldoPendienteActual;

                    ModelState.AddModelError(
                        nameof(vm.Importe),
                        $"El saldo pendiente cambió. Saldo actual: {saldoPendienteActual:C}.");

                    await CargarOpciones(
                        vm,
                        venta.EmpresaId);

                    return View(vm);
                }

                var cobro =
                    new CobroVenta
                    {
                        VentaId =
                            venta.Id,

                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        MedioPagoId =
                            vm.MedioPagoId,

                        // Turno operativo del usuario,
                        // aunque el movimiento no afecte arqueo.
                        TurnoCajaId =
                            turnoOperativo?.Id,

                        UsuarioId =
                            usuario.Id,

                        Fecha =
                            DateTime.Now,

                        Importe =
                            vm.Importe,

                        Estado =
                            EstadoCobro.Activo,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                _context.CobrosVenta.Add(
                    cobro);

                await _context.SaveChangesAsync();

                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.CobroVenta,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            cobro.Fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        // Solo impacta arqueo si esa caja
                        // trabaja con el turno.
                        TurnoCajaId =
                            turnoMovimientoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Cobro de venta #{venta.Id}",

                        Observaciones =
                            null,

                        CobroVentaId =
                            cobro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    vm.Importe == saldoPendienteActual
                        ? "La venta quedó completamente cobrada."
                        : "Cobro registrado correctamente.";

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
                    "Ocurrió un error al registrar el cobro.");

                await CargarOpciones(
                    vm,
                    venta.EmpresaId);

                return View(vm);
            }
        }
        // GET: CobroVenta/Anular/5
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

            IQueryable<CobroVenta> consulta =
                _context.CobrosVenta
                    .AsNoTracking()
                    .Include(c => c.MedioPago)
                    .Include(c => c.Venta);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
            }

            var cobro =
                await consulta
                    .FirstOrDefaultAsync(c =>
                        c.Id == id);

            if (cobro == null)
            {
                return NotFound();
            }

            if (cobro.Estado != EstadoCobro.Activo)
            {
                TempData["Error"] =
                    "El cobro ya se encuentra anulado.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = cobro.VentaId });
            }

            var movimiento =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        m.CobroVentaId == cobro.Id &&
                        m.Tipo == TipoMovimientoCaja.CobroVenta);

            if (movimiento == null)
            {
                TempData["Error"] =
                    "No se encontró el movimiento de caja asociado al cobro.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = cobro.VentaId });
            }

            bool yaRevertido =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.MovimientoOrigenId == movimiento.Id);

            if (yaRevertido)
            {
                TempData["Error"] =
                    "El movimiento asociado a este cobro ya fue revertido.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = cobro.VentaId });
            }

            var vm =
                new AnularCobroVentaVM
                {
                    CobroVentaId =
                        cobro.Id,

                    VentaId =
                        cobro.VentaId,

                    Importe =
                        cobro.Importe,

                    MedioPagoNombre =
                        cobro.MedioPago.Nombre
                };

            return View(vm);
        }
        // POST: CobroVenta/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(AnularCobroVentaVM vm)
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

            IQueryable<CobroVenta> consulta =
                _context.CobrosVenta
                    .Include(c => c.MedioPago)
                    .Include(c => c.Venta);

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
            }

            var cobro =
                await consulta
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CobroVentaId);

            if (cobro == null)
            {
                return NotFound();
            }

            vm.VentaId =
                cobro.VentaId;

            vm.Importe =
                cobro.Importe;

            vm.MedioPagoNombre =
                cobro.MedioPago.Nombre;

            if (cobro.Estado != EstadoCobro.Activo)
            {
                ModelState.AddModelError(
                    "",
                    "El cobro ya se encuentra anulado.");
            }

            var movimiento =
                await _context.MovimientosCaja
                    .FirstOrDefaultAsync(m =>
                        m.CobroVentaId == cobro.Id &&
                        m.Tipo == TipoMovimientoCaja.CobroVenta);

            if (movimiento == null)
            {
                ModelState.AddModelError(
                    "",
                    "No se encontró el movimiento de caja asociado al cobro.");
            }

            bool yaRevertido = false;

            if (movimiento != null)
            {
                yaRevertido =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.MovimientoOrigenId == movimiento.Id);

                if (yaRevertido)
                {
                    ModelState.AddModelError(
                        "",
                        "El movimiento asociado a este cobro ya fue revertido.");
                }
            }

            bool puedeAnularCobro =
                await _ventaSaldoService
                    .PuedeAnularCobro(
                        cobro.VentaId,
                        cobro.Importe);

            if (!puedeAnularCobro)
            {
                ModelState.AddModelError(
                    "",
                    "No se puede anular este cobro porque existen reintegros activos asociados a la venta.");
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
                bool puedeAnularCobroActual =
                    await _ventaSaldoService
                        .PuedeAnularCobro(
                            cobro.VentaId,
                            cobro.Importe);

                if (!puedeAnularCobroActual)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "No se puede anular el cobro porque la venta posee reintegros activos que dependen de ese importe.");

                    return View(vm);
                }       

                cobro.Estado =
                    EstadoCobro.Anulado;

                cobro.FechaAnulacion =
                    DateTime.Now;

                cobro.UsuarioAnulacionId =
                    usuario.Id;

                cobro.MotivoAnulacion =
                    vm.Motivo;

                var movimientoReversion =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            movimiento!.EmpresaId,

                        CajaId =
                            movimiento.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReversionCobroVenta,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            movimiento.Importe,

                        Fecha =
                            DateTime.Now,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            movimiento.MedioPagoId,

                        TurnoCajaId =
                            movimiento.TurnoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reversión de cobro de venta #{cobro.VentaId}",

                        Observaciones =
                            vm.Motivo,

                        MovimientoOrigenId =
                            movimiento.Id,

                        CobroVentaId =
                            cobro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimientoReversion);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Cobro anulado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = cobro.VentaId });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al anular el cobro.");

                return View(vm);
            }
        }
        // GET: CobroVenta/GetCajasPorMedioPago
        [HttpGet]
        public async Task<IActionResult> GetCajasPorMedioPago(int ventaId, int medioPagoId)
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

            IQueryable<Venta> consultaVenta =
                _context.Ventas
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consultaVenta =
                    consultaVenta.Where(v =>
                        v.EmpresaId == usuario.EmpresaId);
            }

            var venta =
                await consultaVenta
                    .FirstOrDefaultAsync(v =>
                        v.Id == ventaId);

            if (venta == null)
            {
                return NotFound();
            }

            var cajas =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm =>
                        cm.MedioPagoId == medioPagoId &&
                        cm.MedioPago.EmpresaId == venta.EmpresaId &&
                        cm.MedioPago.Estado &&
                        cm.Caja.EmpresaId == venta.EmpresaId &&
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
        private async Task CargarOpciones(RegistrarCobroVentaVM vm, int empresaId)
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