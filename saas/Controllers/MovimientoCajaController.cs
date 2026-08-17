using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class MovimientoCajaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public MovimientoCajaController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MovimientoCaja
        public async Task<IActionResult> Index(
            int? cajaId = null,
            int? medioPagoId = null,
            int? categoriaGastoId = null,
            int? turnoCajaId = null,
            string? usuarioId = null,
            TipoMovimientoCaja? tipo = null,
            DireccionMovimientoCaja? direccion = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<MovimientoCaja> consulta =
                _context.MovimientosCaja
                    .AsNoTracking()
                    .Include(m => m.Caja)
                    .Include(m => m.Usuario)
                    .Include(m => m.MedioPago)
                    .Include(m => m.CategoriaGasto);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(m =>
                    m.EmpresaId == usuario.EmpresaId);

                empresaId = null;
            }
            else if (empresaId.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.EmpresaId == empresaId.Value);
            }

            if (cajaId.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.CajaId == cajaId.Value);
            }

            if (medioPagoId.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.MedioPagoId == medioPagoId.Value);
            }

            if (categoriaGastoId.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.CategoriaGastoId ==
                    categoriaGastoId.Value);
            }

            if (turnoCajaId.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.TurnoCajaId ==
                    turnoCajaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(usuarioId))
            {
                consulta = consulta.Where(m =>
                    m.UsuarioId == usuarioId);
            }

            if (tipo.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Tipo == tipo.Value);
            }

            if (direccion.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Direccion == direccion.Value);
            }

            if (fechaDesde.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Fecha >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                var hastaExclusivo =
                    fechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(m =>
                    m.Fecha < hastaExclusivo);
            }

            var movimientos = await consulta
                .OrderByDescending(m => m.Fecha)
                .Select(m => new MovimientoCajaResumenVM
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    CajaNombre = m.Caja.Nombre,
                    Tipo = m.Tipo,
                    Direccion = m.Direccion,
                    Importe = m.Importe,
                    UsuarioNombre =
                        m.Usuario.UserName ?? "",
                    MedioPagoNombre =
                        m.MedioPago != null
                            ? m.MedioPago.Nombre
                            : null,
                    CategoriaGastoNombre =
                        m.CategoriaGasto != null
                            ? m.CategoriaGasto.Nombre
                            : null,
                    Concepto = m.Concepto,
                    Observaciones = m.Observaciones,
                    TurnoCajaId = m.TurnoCajaId,
                    MovimientoOrigenId =
                        m.MovimientoOrigenId,
                    EsReversion =
                        m.MovimientoOrigenId.HasValue
                })
                .ToListAsync();

            var vm = new MovimientoCajaIndexVM
            {
                Movimientos = movimientos,

                CajaId = cajaId,
                MedioPagoId = medioPagoId,
                CategoriaGastoId = categoriaGastoId,
                TurnoCajaId = turnoCajaId,
                UsuarioId = usuarioId,
                Tipo = tipo,
                Direccion = direccion,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                EmpresaId =
                    esSuperAdmin ? empresaId : null,

                TotalIngresos =
                    movimientos
                        .Where(m =>
                            m.Direccion ==
                            DireccionMovimientoCaja.Ingreso)
                        .Sum(m => m.Importe),

                TotalEgresos =
                    movimientos
                        .Where(m =>
                            m.Direccion ==
                            DireccionMovimientoCaja.Egreso)
                        .Sum(m => m.Importe)
            };

            vm.NetoPeriodo =
                vm.TotalIngresos -
                vm.TotalEgresos;

            await CargarOpcionesIndex(
                vm,
                esSuperAdmin,
                usuario.EmpresaId);

            return View(vm);
        }

        // GET: MovimientoCaja/IngresoManual
        [HttpGet]
        public async Task<IActionResult> IngresoManual()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin"))
            {
                TempData["Error"] =
                    "Para registrar movimientos debe operar dentro de una empresa específica.";

                return RedirectToAction(nameof(Index));
            }

            var vm = new IngresoManualVM();

            await CargarOpcionesIngreso(
                vm,
                usuario.EmpresaId);

            return View(vm);
        }

        // POST: MovimientoCaja/IngresoManual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IngresoManual(
            IngresoManualVM vm)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesIngreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == vm.CajaId &&
                    c.EmpresaId == usuario.EmpresaId &&
                    c.Estado);

            if (caja == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no es válida.");
            }

            bool medioPagoValido =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .AnyAsync(cm =>
                        cm.CajaId == vm.CajaId &&
                        cm.MedioPagoId == vm.MedioPagoId &&
                        cm.Caja.EmpresaId == usuario.EmpresaId &&
                        cm.Caja.Estado &&
                        cm.MedioPago.Estado);

            if (!medioPagoValido)
            {
                ModelState.AddModelError(
                    nameof(vm.MedioPagoId),
                    "El medio de pago seleccionado no es válido para esta caja.");
            }

            TurnoCaja? turno = null;

            if (caja != null &&
                caja.PermiteTurnos)
            {
                turno = await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.CajaId == caja.Id &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

                if (turno == null)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        "Debe tener un turno abierto propio para operar esta caja.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesIngreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            vm.Concepto = vm.Concepto.Trim();

            vm.Observaciones =
                string.IsNullOrWhiteSpace(
                    vm.Observaciones)
                    ? null
                    : vm.Observaciones.Trim();

            try
            {
                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            usuario.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.IngresoManual,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            DateTime.Now,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turno?.Id,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            vm.Concepto,

                        Observaciones =
                            vm.Observaciones
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Ingreso manual registrado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = movimiento.Id });
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el ingreso.");

                await CargarOpcionesIngreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }
        }

        // GET: MovimientoCaja/EgresoManual
        [HttpGet]
        public async Task<IActionResult> EgresoManual()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin"))
            {
                TempData["Error"] =
                    "Para registrar movimientos debe operar dentro de una empresa específica.";

                return RedirectToAction(nameof(Index));
            }

            var vm = new EgresoManualVM();

            await CargarOpcionesEgreso(
                vm,
                usuario.EmpresaId);

            return View(vm);
        }

        // POST: MovimientoCaja/EgresoManual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EgresoManual(
            EgresoManualVM vm)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesEgreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == vm.CajaId &&
                    c.EmpresaId == usuario.EmpresaId &&
                    c.Estado);

            if (caja == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no es válida.");
            }

            bool medioPagoValido =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .AnyAsync(cm =>
                        cm.CajaId == vm.CajaId &&
                        cm.MedioPagoId == vm.MedioPagoId &&
                        cm.Caja.EmpresaId == usuario.EmpresaId &&
                        cm.Caja.Estado &&
                        cm.MedioPago.Estado);

            if (!medioPagoValido)
            {
                ModelState.AddModelError(
                    nameof(vm.MedioPagoId),
                    "El medio de pago seleccionado no es válido para esta caja.");
            }

            bool categoriaValida =
                await _context.CategoriasGasto
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.Id == vm.CategoriaGastoId &&
                        c.EmpresaId == usuario.EmpresaId &&
                        c.Estado);

            if (!categoriaValida)
            {
                ModelState.AddModelError(
                    nameof(vm.CategoriaGastoId),
                    "La categoría de gasto seleccionada no es válida.");
            }

            TurnoCaja? turno = null;

            if (caja != null &&
                caja.PermiteTurnos)
            {
                turno = await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.CajaId == caja.Id &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

                if (turno == null)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        "Debe tener un turno abierto propio para operar esta caja.");
                }
            }

            decimal saldoDisponible = 0;

            if (caja != null)
            {
                saldoDisponible =
                    await CalcularSaldoDisponibleCaja(
                        caja,
                        usuario.Id);
            }

            vm.SaldoDisponible =
                saldoDisponible;

            if (vm.Importe >
                saldoDisponible)
            {
                ModelState.AddModelError(
                    nameof(vm.Importe),
                    "El importe supera el saldo disponible de la caja.");
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesEgreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            vm.Concepto = vm.Concepto.Trim();

            vm.Observaciones =
                string.IsNullOrWhiteSpace(
                    vm.Observaciones)
                    ? null
                    : vm.Observaciones.Trim();

            try
            {
                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            usuario.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.EgresoManual,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            DateTime.Now,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        TurnoCajaId =
                            turno?.Id,

                        CategoriaGastoId =
                            vm.CategoriaGastoId,

                        Concepto =
                            vm.Concepto,

                        Observaciones =
                            vm.Observaciones
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Egreso manual registrado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = movimiento.Id });
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el egreso.");

                await CargarOpcionesEgreso(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }
        }
        // GET: MovimientoCaja/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<MovimientoCaja> consulta =
                _context.MovimientosCaja
                    .AsNoTracking()
                    .Include(m => m.Empresa)
                    .Include(m => m.Caja)
                    .Include(m => m.Usuario)
                    .Include(m => m.MedioPago)
                    .Include(m => m.CategoriaGasto);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(m =>
                    m.EmpresaId == usuario.EmpresaId);
            }

            var movimiento = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            var reversiones =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .Where(m =>
                        m.MovimientoOrigenId == movimiento.Id)
                    .Include(m => m.Caja)
                    .Include(m => m.Usuario)
                    .Include(m => m.MedioPago)
                    .Include(m => m.CategoriaGasto)
                    .OrderByDescending(m => m.Fecha)
                    .Select(m => new MovimientoCajaResumenVM
                    {
                        Id = m.Id,
                        Fecha = m.Fecha,
                        CajaNombre = m.Caja.Nombre,
                        Tipo = m.Tipo,
                        Direccion = m.Direccion,
                        Importe = m.Importe,

                        UsuarioNombre =
                            m.Usuario.UserName ?? "",

                        MedioPagoNombre =
                            m.MedioPago != null
                                ? m.MedioPago.Nombre
                                : null,

                        CategoriaGastoNombre =
                            m.CategoriaGasto != null
                                ? m.CategoriaGasto.Nombre
                                : null,

                        Concepto = m.Concepto,
                        Observaciones = m.Observaciones,
                        TurnoCajaId = m.TurnoCajaId,
                        MovimientoOrigenId =
                            m.MovimientoOrigenId,

                        EsReversion = true
                    })
                    .ToListAsync();

            var vm = new MovimientoCajaDetailsVM
            {
                Id = movimiento.Id,
                EmpresaNombre = movimiento.Empresa.Nombre,
                CajaNombre = movimiento.Caja.Nombre,

                Tipo = movimiento.Tipo,
                Direccion = movimiento.Direccion,
                Importe = movimiento.Importe,
                Fecha = movimiento.Fecha,

                UsuarioNombre =
                    movimiento.Usuario.UserName ?? "",

                MedioPagoNombre =
                    movimiento.MedioPago?.Nombre,

                TurnoCajaId =
                    movimiento.TurnoCajaId,

                CategoriaGastoNombre =
                    movimiento.CategoriaGasto?.Nombre,

                Concepto =
                    movimiento.Concepto,

                Observaciones =
                    movimiento.Observaciones,

                MovimientoOrigenId =
                    movimiento.MovimientoOrigenId,

                EsReversion =
                    movimiento.MovimientoOrigenId.HasValue,

                FueRevertido =
                    reversiones.Any(),

                Reversiones =
                    reversiones,

                CobroVentaId =
                    movimiento.CobroVentaId,

                PagoProveedorId =
                    movimiento.PagoProveedorId,

                ReintegroVentaId =
                    movimiento.ReintegroVentaId,

                ReintegroProveedorId =
                    movimiento.ReintegroProveedorId,

                TransferenciaCajaId =
                    movimiento.TransferenciaCajaId
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> GetMediosPagoPorCaja(
    int cajaId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == cajaId);

            if (caja == null)
            {
                return NotFound();
            }

            if (!esSuperAdmin &&
                caja.EmpresaId != usuario.EmpresaId)
            {
                return Forbid();
            }

            var medios =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm =>
                        cm.CajaId == cajaId &&
                        cm.MedioPago.Estado)
                    .OrderBy(cm =>
                        cm.MedioPago.Nombre)
                    .Select(cm => new
                    {
                        id = cm.MedioPagoId,
                        nombre = cm.MedioPago.Nombre
                    })
                    .ToListAsync();

            return Json(medios);
        }
        [HttpGet]
        public async Task<IActionResult> GetSaldoCaja(
    int cajaId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            var caja = await _context.Cajas
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == cajaId);

            if (caja == null)
            {
                return NotFound();
            }

            if (!esSuperAdmin &&
                caja.EmpresaId != usuario.EmpresaId)
            {
                return Forbid();
            }

            decimal saldo =
                await CalcularSaldoDisponibleCaja(
                    caja,
                    usuario.Id);

            return Json(new
            {
                saldo
            });
        }

        //Helper methods
        private async Task<decimal> CalcularSaldoDisponibleCaja(
    Caja caja,
    string usuarioId)
        {
            if (caja.PermiteTurnos)
            {
                var turno = await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.CajaId == caja.Id &&
                        t.UsuarioAperturaId == usuarioId &&
                        t.Estado == EstadoTurnoCaja.Abierto);

                if (turno == null)
                {
                    return 0;
                }

                decimal netoTurno =
                    await _context.MovimientosCaja
                        .AsNoTracking()
                        .Where(m =>
                            m.TurnoCajaId == turno.Id)
                        .SumAsync(m =>
                            m.Direccion ==
                            DireccionMovimientoCaja.Ingreso
                                ? m.Importe
                                : -m.Importe);

                return turno.FondoFijoAplicado
                    + netoTurno;
            }

            return await _context.MovimientosCaja
                .AsNoTracking()
                .Where(m =>
                    m.CajaId == caja.Id)
                .SumAsync(m =>
                    m.Direccion ==
                    DireccionMovimientoCaja.Ingreso
                        ? m.Importe
                        : -m.Importe);
        }

        private async Task CargarOpcionesIngreso(
            IngresoManualVM vm,
            int empresaId)
        {
            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c => c.Nombre)
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
                    .OrderBy(m => m.Nombre)
                    .Select(m =>
                        new MedioPagoOpcionSimpleVM
                        {
                            Id = m.Id,
                            Nombre = m.Nombre
                        })
                    .ToListAsync();
        }

        private async Task CargarOpcionesEgreso(
            EgresoManualVM vm,
            int empresaId)
        {
            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c => c.Nombre)
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
                    .OrderBy(m => m.Nombre)
                    .Select(m =>
                        new MedioPagoOpcionSimpleVM
                        {
                            Id = m.Id,
                            Nombre = m.Nombre
                        })
                    .ToListAsync();

            vm.CategoriasDisponibles =
                await _context.CategoriasGasto
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c => c.Nombre)
                    .Select(c =>
                        new CategoriaGastoOpcionVM
                        {
                            Id = c.Id,
                            Nombre = c.Nombre
                        })
                    .ToListAsync();
        }

        private async Task CargarOpcionesIndex(
            MovimientoCajaIndexVM vm,
            bool esSuperAdmin,
            int empresaUsuarioId)
        {
            int? empresaId =
                esSuperAdmin
                    ? vm.EmpresaId
                    : empresaUsuarioId;

            if (!empresaId.HasValue)
            {
                return;
            }

            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId.Value)
                    .OrderBy(c => c.Nombre)
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
                        m.EmpresaId == empresaId.Value)
                    .OrderBy(m => m.Nombre)
                    .Select(m =>
                        new MedioPagoOpcionSimpleVM
                        {
                            Id = m.Id,
                            Nombre = m.Nombre
                        })
                    .ToListAsync();

            vm.CategoriasDisponibles =
                await _context.CategoriasGasto
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId.Value)
                    .OrderBy(c => c.Nombre)
                    .Select(c =>
                        new CategoriaGastoOpcionVM
                        {
                            Id = c.Id,
                            Nombre = c.Nombre
                        })
                    .ToListAsync();

            vm.UsuariosDisponibles =
                await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.EmpresaId == empresaId.Value)
                    .OrderBy(u => u.UserName)
                    .Select(u =>
                        new UsuarioOpcionVM
                        {
                            Id = u.Id,
                            Nombre =
                                u.UserName ?? ""
                        })
                    .ToListAsync();
        }
    }
}