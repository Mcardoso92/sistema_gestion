using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel;
using saas.ViewModel.ReintegroProveedor;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ReintegroProveedorController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly CompraSaldoService _compraSaldoService;

        public ReintegroProveedorController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            CompraSaldoService compraSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _compraSaldoService = compraSaldoService;
        }
        // GET: ReintegroProveedor/Registrar?compraId=5
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
                    .Include(c => c.Proveedor);

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
                    "No se pueden registrar reintegros sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            decimal pendienteRecuperar =
                await _compraSaldoService.ObtenerPendienteRecuperar(
                    compra.Id,
                    compra.Total);

            if (pendienteRecuperar <= 0)
            {
                TempData["Error"] =
                    "La compra no tiene un importe pendiente de recuperar del proveedor.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            var vm = new RegistrarReintegroProveedorVM
            {
                CompraId =
                    compra.Id,

                ImporteDisponible =
                    pendienteRecuperar,

                Importe =
                    pendienteRecuperar
            };

            await CargarOpciones(
                vm,
                compra.EmpresaId);

            return View(vm);
        }
        // POST: ReintegroProveedor/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarReintegroProveedorVM vm)
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
                    "No se pueden registrar reintegros sobre una compra anulada.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            decimal pendienteRecuperar =
                await _compraSaldoService
                    .ObtenerPendienteRecuperar(
                        compra.Id,
                        compra.Total);

            vm.ImporteDisponible =
                pendienteRecuperar;

            if (pendienteRecuperar <= 0)
            {
                TempData["Error"] =
                    "La compra no tiene un importe pendiente de recuperar del proveedor.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = compra.Id });
            }

            if (vm.Importe > pendienteRecuperar)
            {
                ModelState.AddModelError(
                    nameof(vm.Importe),
                    "El importe no puede superar el monto pendiente de recuperar.");
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
                        "La compra fue anulada antes de registrar el reintegro.";

                    return RedirectToAction(
                        "Details",
                        "Compra",
                        new { id = compraActual.Id });
                }

                decimal pendienteRecuperarActual =
                    await _compraSaldoService
                        .ObtenerPendienteRecuperar(
                            compraActual.Id,
                            compraActual.Total);

                if (pendienteRecuperarActual <= 0)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] =
                        "La compra ya no tiene un importe pendiente de recuperar.";

                    return RedirectToAction(
                        "Details",
                        "Compra",
                        new { id = compraActual.Id });
                }

                if (vm.Importe > pendienteRecuperarActual)
                {
                    await transaccion.RollbackAsync();

                    vm.ImporteDisponible =
                        pendienteRecuperarActual;

                    ModelState.AddModelError(
                        nameof(vm.Importe),
                        $"El importe pendiente cambió. Disponible actual: {pendienteRecuperarActual:C}.");

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

                DateTime fechaReintegro =
                    DateTime.Now;

                var reintegro =
                    new ReintegroProveedor
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
                            fechaReintegro,

                        Importe =
                            vm.Importe,

                        Estado =
                            EstadoReintegro.Activo,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                _context.ReintegrosProveedor.Add(
                    reintegro);

                await _context.SaveChangesAsync();

                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            compraActual.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReintegroProveedor,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            fechaReintegro,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turnoMovimientoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reintegro de proveedor por compra #{compraActual.Id}",

                        Observaciones =
                            null,

                        ReintegroProveedorId =
                            reintegro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Reintegro registrado correctamente.";

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
                    "Ocurrió un error al registrar el reintegro.");

                vm.ImporteDisponible =
                    await _compraSaldoService
                        .ObtenerPendienteRecuperar(
                            compra.Id,
                            compra.Total);

                await CargarOpciones(
                    vm,
                    compra.EmpresaId);

                return View(vm);
            }
        }
        // GET: ReintegroProveedor/Anular/5
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

            IQueryable<ReintegroProveedor> consulta =
                _context.ReintegrosProveedor
                    .AsNoTracking();

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

            if (reintegro.Estado ==
                EstadoReintegro.Anulado)
            {
                TempData["Error"] =
                    "El reintegro ya se encuentra anulado.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = reintegro.CompraId });
            }

            var vm =
                new AnularReintegroProveedorVM
                {
                    ReintegroProveedorId =
                        reintegro.Id,

                    CompraId =
                        reintegro.CompraId,

                    Importe =
                        reintegro.Importe
                };

            return View(vm);
        }
        // POST: ReintegroProveedor/Anular
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(AnularReintegroProveedorVM vm)
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

            IQueryable<ReintegroProveedor> consulta =
                _context.ReintegrosProveedor
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(r =>
                        r.EmpresaId == usuario.EmpresaId);
            }

            var reintegro =
                await consulta
                    .FirstOrDefaultAsync(r =>
                        r.Id == vm.ReintegroProveedorId);

            if (reintegro == null)
            {
                return NotFound();
            }

            vm.CompraId =
                reintegro.CompraId;

            vm.Importe =
                reintegro.Importe;

            if (reintegro.Estado ==
                EstadoReintegro.Anulado)
            {
                ModelState.AddModelError(
                    "",
                    "El reintegro ya se encuentra anulado.");
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
                var reintegroActual =
                    await _context.ReintegrosProveedor
                        .FirstOrDefaultAsync(r =>
                            r.Id == reintegro.Id &&
                            r.EmpresaId == reintegro.EmpresaId);

                if (reintegroActual == null)
                {
                    await transaccion.RollbackAsync();

                    return NotFound();
                }

                if (reintegroActual.Estado ==
                    EstadoReintegro.Anulado)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "El reintegro ya fue anulado.");

                    return View(vm);
                }

                var movimientoOriginal =
                    await _context.MovimientosCaja
                        .FirstOrDefaultAsync(m =>
                            m.ReintegroProveedorId ==
                                reintegroActual.Id &&
                            m.Tipo ==
                                TipoMovimientoCaja.ReintegroProveedor);

                if (movimientoOriginal == null)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "No se encontró el movimiento de caja original del reintegro.");

                    return View(vm);
                }

                bool yaRevertido =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .AnyAsync(m =>
                            m.MovimientoOrigenId ==
                                movimientoOriginal.Id &&
                            m.Tipo ==
                                TipoMovimientoCaja.ReversionReintegroProveedor);

                if (yaRevertido)
                {
                    await transaccion.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "El reintegro ya posee una reversión de caja.");

                    return View(vm);
                }

                DateTime fechaAnulacion =
                    DateTime.Now;

                reintegroActual.Estado =
                    EstadoReintegro.Anulado;

                reintegroActual.FechaAnulacion =
                    fechaAnulacion;

                reintegroActual.UsuarioAnulacionId =
                    usuario.Id;

                reintegroActual.MotivoAnulacion =
                    vm.Motivo;

                var movimientoReversion =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            reintegroActual.EmpresaId,

                        CajaId =
                            reintegroActual.CajaId,

                        Tipo =
                            TipoMovimientoCaja.ReversionReintegroProveedor,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            reintegroActual.Importe,

                        Fecha =
                            fechaAnulacion,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            reintegroActual.MedioPagoId,

                        TurnoCajaId =
                            reintegroActual.TurnoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Reversión de reintegro proveedor #{reintegroActual.Id}",

                        Observaciones =
                            vm.Motivo,

                        MovimientoOrigenId =
                            movimientoOriginal.Id
                    };

                _context.MovimientosCaja.Add(
                    movimientoReversion);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Reintegro anulado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Compra",
                    new { id = reintegroActual.CompraId });
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
        // GET: ReintegroProveedor/GetCajasPorMedioPago
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
        private async Task CargarOpciones(
    RegistrarReintegroProveedorVM vm,
    int empresaId)
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