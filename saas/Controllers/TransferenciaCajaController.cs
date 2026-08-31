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
    public class TransferenciaCajaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly CajaSaldoService _cajaSaldoService;

        public TransferenciaCajaController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            CajaSaldoService cajaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _cajaSaldoService = cajaSaldoService;
        }

        // GET: TransferenciaCaja
        public async Task<IActionResult> Index(
            int? cajaOrigenId = null,
            int? cajaDestinoId = null,
            EstadoTransferenciaCaja? estado = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? empresaId = null,
            int pagina = 1)
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

            IQueryable<TransferenciaCaja> consulta =
                _context.TransferenciasCaja
                    .AsNoTracking()
                    .Include(t => t.CajaOrigen)
                    .Include(t => t.CajaDestino)
                    .Include(t => t.Usuario)
                    .Include(t => t.UsuarioAnulacion);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);

                empresaId = null;
            }
            else if (empresaId.HasValue)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == empresaId.Value);
            }

            if (cajaOrigenId.HasValue)
            {
                consulta = consulta.Where(t =>
                    t.CajaOrigenId == cajaOrigenId.Value);
            }

            if (cajaDestinoId.HasValue)
            {
                consulta = consulta.Where(t =>
                    t.CajaDestinoId == cajaDestinoId.Value);
            }

            if (estado.HasValue)
            {
                consulta = consulta.Where(t =>
                    t.Estado == estado.Value);
            }

            if (fechaDesde.HasValue)
            {
                consulta = consulta.Where(t =>
                    t.Fecha >= fechaDesde.Value.Date);
            }

            if (fechaHasta.HasValue)
            {
                var hastaExclusivo =
                    fechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(t =>
                    t.Fecha < hastaExclusivo);
            }

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalTransferencias = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalTransferencias / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalTransferencias;

            var transferencias =
                await consulta
                    .OrderByDescending(t => t.Fecha)
                    .Skip((pagina - 1) * tamanioPagina)
                    .Take(tamanioPagina)
                    .Select(t => new TransferenciaCajaResumenVM
                    {
                        Id = t.Id,
                        Fecha = t.Fecha,
                        CajaOrigenNombre =
                            t.CajaOrigen.Nombre,
                        CajaDestinoNombre =
                            t.CajaDestino.Nombre,
                        Importe = t.Importe,
                        Motivo = t.Motivo,
                        UsuarioNombre =
                            t.Usuario.UserName ?? "",
                        TurnoCajaId =
                            t.TurnoCajaId,
                        Estado =
                            t.Estado,
                        FechaAnulacion =
                            t.FechaAnulacion,
                        UsuarioAnulacionNombre =
                            t.UsuarioAnulacion != null
                                ? t.UsuarioAnulacion.UserName
                                : null,
                        MotivoAnulacion =
                            t.MotivoAnulacion
                    })
                    .ToListAsync();

            ViewBag.CajaOrigenId = cajaOrigenId;
            ViewBag.CajaDestinoId = cajaDestinoId;
            ViewBag.Estado = estado;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.EmpresaId =
                esSuperAdmin ? empresaId : null;

            int? empresaFiltro =
                esSuperAdmin
                    ? empresaId
                    : usuario.EmpresaId;

            if (empresaFiltro.HasValue)
            {
                ViewBag.Cajas =
                    await _context.Cajas
                        .AsNoTracking()
                        .Where(c =>
                            c.EmpresaId == empresaFiltro.Value)
                        .OrderBy(c => c.Nombre)
                        .ToListAsync();
            }

            if (esSuperAdmin)
            {
                ViewBag.Empresas =
                    await _context.Empresas
                        .AsNoTracking()
                        .Where(e => e.Estado)
                        .OrderBy(e => e.Nombre)
                        .ToListAsync();
            }

            return View(transferencias);
        }

        // GET: TransferenciaCaja/Create
        [HttpGet]
        public async Task<IActionResult> Create()
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
                    "Para realizar una transferencia debe operar dentro de una empresa específica.";

                return RedirectToAction(nameof(Index));
            }

            var vm = new TransferenciaCajaCreateVM();

            await CargarOpcionesTransferencia(
                vm,
                usuario.EmpresaId);

            return View(vm);
        }

        // POST: TransferenciaCaja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TransferenciaCajaCreateVM vm)
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

            if (vm.CajaOrigenId ==
                vm.CajaDestinoId)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaDestinoId),
                    "La caja destino debe ser distinta de la caja origen.");
            }

            var cajaOrigen =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CajaOrigenId &&
                        c.EmpresaId == usuario.EmpresaId &&
                        c.Estado);

            if (cajaOrigen == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaOrigenId),
                    "La caja de origen no es válida.");
            }

            var cajaDestino =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CajaDestinoId &&
                        c.EmpresaId == usuario.EmpresaId &&
                        c.Estado);

            if (cajaDestino == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaDestinoId),
                    "La caja de destino no es válida.");
            }

            TurnoCaja? turnoOrigen = null;

            if (cajaOrigen != null &&
                cajaOrigen.PermiteTurnos)
            {
                turnoOrigen =
                    await _context.TurnosCaja
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t =>
                            t.CajaId == cajaOrigen.Id &&
                            t.UsuarioAperturaId == usuario.Id &&
                            t.Estado == EstadoTurnoCaja.Abierto);

                if (turnoOrigen == null)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaOrigenId),
                        "Debe tener un turno abierto propio para transferir dinero desde esta caja.");
                }
            }

            decimal saldoDisponible = 0;

            if (cajaOrigen != null)
            {
                saldoDisponible =
                    await _cajaSaldoService
                        .CalcularSaldoDisponible(
                            cajaOrigen,
                            usuario.Id);
            }

            vm.SaldoDisponibleOrigen =
                saldoDisponible;

            if (vm.Importe >
                saldoDisponible)
            {
                ModelState.AddModelError(
                    nameof(vm.Importe),
                    "El importe supera el saldo disponible de la caja de origen.");
            }

            if (!ModelState.IsValid)
            {
                await CargarOpcionesTransferencia(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            vm.Motivo =
                vm.Motivo.Trim();

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                decimal saldoDisponibleActual =
                    await _cajaSaldoService
                        .CalcularSaldoDisponible(
                            cajaOrigen!,
                            usuario.Id);

                if (vm.Importe >
                    saldoDisponibleActual)
                {
                    await transaction.RollbackAsync();

                    vm.SaldoDisponibleOrigen =
                        saldoDisponibleActual;

                    ModelState.AddModelError(
                        nameof(vm.Importe),
                        $"El saldo disponible de la caja cambió. Saldo actual: {saldoDisponibleActual:C}.");

                    await CargarOpcionesTransferencia(
                        vm,
                        usuario.EmpresaId);

                    return View(vm);
                }

                var fecha =
                    DateTime.Now;

                var transferencia =
                    new TransferenciaCaja
                    {
                        EmpresaId =
                            usuario.EmpresaId,

                        CajaOrigenId =
                            vm.CajaOrigenId,

                        CajaDestinoId =
                            vm.CajaDestinoId,

                        Importe =
                            vm.Importe,

                        Fecha =
                            fecha,

                        Motivo =
                            vm.Motivo,

                        UsuarioId =
                            usuario.Id,

                        TurnoCajaId =
                            turnoOrigen?.Id,

                        Estado =
                            EstadoTransferenciaCaja.Activa,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                _context.TransferenciasCaja.Add(
                    transferencia);

                await _context.SaveChangesAsync();

                var movimientoSalida =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            usuario.EmpresaId,

                        CajaId =
                            vm.CajaOrigenId,

                        Tipo =
                            TipoMovimientoCaja.TransferenciaSalida,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            null,

                        TurnoCajaId =
                            turnoOrigen?.Id,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Transferencia a caja {cajaDestino!.Nombre}",

                        Observaciones =
                            vm.Motivo,

                        TransferenciaCajaId =
                            transferencia.Id
                    };

                var movimientoEntrada =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            usuario.EmpresaId,

                        CajaId =
                            vm.CajaDestinoId,

                        Tipo =
                            TipoMovimientoCaja.TransferenciaEntrada,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            null,

                        // El turno pertenece solo al origen.
                        TurnoCajaId =
                            null,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Transferencia recibida desde caja {cajaOrigen!.Nombre}",

                        Observaciones =
                            vm.Motivo,

                        TransferenciaCajaId =
                            transferencia.Id
                    };

                _context.MovimientosCaja.AddRange(
                    movimientoSalida,
                    movimientoEntrada);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Transferencia realizada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al realizar la transferencia.");

                await CargarOpcionesTransferencia(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }
        }
        // GET: TransferenciaCaja/Details/5
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

            IQueryable<TransferenciaCaja> consulta =
                _context.TransferenciasCaja
                    .AsNoTracking()
                    .Include(t => t.CajaOrigen)
                    .Include(t => t.CajaDestino)
                    .Include(t => t.Usuario)
                    .Include(t => t.UsuarioAnulacion);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var transferencia =
                await consulta
                    .FirstOrDefaultAsync(t =>
                        t.Id == id);

            if (transferencia == null)
            {
                return NotFound();
            }

            var vm =
                new TransferenciaCajaResumenVM
                {
                    Id = transferencia.Id,
                    Fecha = transferencia.Fecha,

                    CajaOrigenNombre =
                        transferencia.CajaOrigen.Nombre,

                    CajaDestinoNombre =
                        transferencia.CajaDestino.Nombre,

                    Importe =
                        transferencia.Importe,

                    Motivo =
                        transferencia.Motivo,

                    UsuarioNombre =
                        transferencia.Usuario.UserName ?? "",

                    TurnoCajaId =
                        transferencia.TurnoCajaId,

                    Estado =
                        transferencia.Estado,

                    FechaAnulacion =
                        transferencia.FechaAnulacion,

                    UsuarioAnulacionNombre =
                        transferencia.UsuarioAnulacion != null
                            ? transferencia.UsuarioAnulacion.UserName
                            : null,

                    MotivoAnulacion =
                        transferencia.MotivoAnulacion
                };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> GetSaldoCaja(int cajaId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin"))
            {
                return Forbid();
            }

            var caja =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == cajaId &&
                        c.EmpresaId == usuario.EmpresaId &&
                        c.Estado);

            if (caja == null)
            {
                return NotFound();
            }

            decimal saldo =
                await _cajaSaldoService
                    .CalcularSaldoDisponible(
                        caja,
                        usuario.Id);

            return Json(new
            {
                saldo
            });
        }
        // GET: TransferenciaCaja/Anular/5
        [HttpGet]
        public async Task<IActionResult> Anular(int? id)
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

            IQueryable<TransferenciaCaja> consulta =
                _context.TransferenciasCaja
                    .AsNoTracking()
                    .Include(t => t.CajaOrigen)
                    .Include(t => t.CajaDestino)
                    .Include(t => t.Usuario)
                    .Include(t => t.TurnoCaja);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var transferencia =
                await consulta.FirstOrDefaultAsync(t =>
                    t.Id == id);

            if (transferencia == null)
            {
                return NotFound();
            }

            if (transferencia.Estado ==
                EstadoTransferenciaCaja.Anulada)
            {
                TempData["Error"] =
                    "La transferencia ya se encuentra anulada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            if (transferencia.TurnoCajaId.HasValue &&
                transferencia.TurnoCaja != null &&
                transferencia.TurnoCaja.Estado ==
                    EstadoTurnoCaja.Cerrado)
            {
                TempData["Error"] =
                    "No puede anularse una transferencia asociada a un turno ya cerrado. Debe realizar una corrección administrativa posterior.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            var vm = new TransferenciaCajaResumenVM
            {
                Id = transferencia.Id,
                Fecha = transferencia.Fecha,

                CajaOrigenNombre =
                    transferencia.CajaOrigen.Nombre,

                CajaDestinoNombre =
                    transferencia.CajaDestino.Nombre,

                Importe =
                    transferencia.Importe,

                Motivo =
                    transferencia.Motivo,

                UsuarioNombre =
                    transferencia.Usuario.UserName ?? "",

                TurnoCajaId =
                    transferencia.TurnoCajaId,

                Estado =
                    transferencia.Estado
            };

            return View(vm);
        }
        // POST: TransferenciaCaja/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id, string motivoAnulacion)
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

            IQueryable<TransferenciaCaja> consulta =
                _context.TransferenciasCaja
                    .Include(t => t.CajaOrigen)
                    .Include(t => t.CajaDestino)
                    .Include(t => t.Usuario)
                    .Include(t => t.TurnoCaja);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var transferencia =
                await consulta.FirstOrDefaultAsync(t =>
                    t.Id == id);

            if (transferencia == null)
            {
                return NotFound();
            }

            if (transferencia.Estado ==
                EstadoTransferenciaCaja.Anulada)
            {
                TempData["Error"] =
                    "La transferencia ya se encuentra anulada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            if (transferencia.TurnoCajaId.HasValue &&
                transferencia.TurnoCaja != null &&
                transferencia.TurnoCaja.Estado ==
                    EstadoTurnoCaja.Cerrado)
            {
                TempData["Error"] =
                    "No puede anularse una transferencia asociada a un turno ya cerrado. Debe realizar una corrección administrativa posterior.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            if (string.IsNullOrWhiteSpace(
                motivoAnulacion))
            {
                ViewBag.Error =
                    "Debe indicar el motivo de la anulación.";

                return View(
                    CrearResumenTransferencia(
                        transferencia));
            }

            motivoAnulacion =
                motivoAnulacion.Trim();

            if (motivoAnulacion.Length > 500)
            {
                ViewBag.Error =
                    "El motivo de anulación no puede superar los 500 caracteres.";

                return View(
                    CrearResumenTransferencia(
                        transferencia));
            }

            var movimientosOriginales =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .Where(m =>
                        m.TransferenciaCajaId ==
                            transferencia.Id &&
                        !m.MovimientoOrigenId.HasValue)
                    .ToListAsync();

            var movimientoSalida =
                movimientosOriginales
                    .FirstOrDefault(m =>
                        m.Tipo ==
                        TipoMovimientoCaja.TransferenciaSalida);

            var movimientoEntrada =
                movimientosOriginales
                    .FirstOrDefault(m =>
                        m.Tipo ==
                        TipoMovimientoCaja.TransferenciaEntrada);

            if (movimientoSalida == null ||
                movimientoEntrada == null)
            {
                TempData["Error"] =
                    "No se encontraron los movimientos originales de la transferencia.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            bool yaTieneReversion =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.MovimientoOrigenId ==
                            movimientoSalida.Id ||
                        m.MovimientoOrigenId ==
                            movimientoEntrada.Id);

            if (yaTieneReversion)
            {
                TempData["Error"] =
                    "Los movimientos de esta transferencia ya fueron revertidos.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                await _context.Entry(transferencia).ReloadAsync();

                if (transferencia.Estado == EstadoTransferenciaCaja.Anulada)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] = "La transferencia ya fue anulada por otra operación.";

                    return RedirectToAction(nameof(Details), new { id = transferencia.Id });
                }

                bool reversionRegistradaDuranteLaOperacion = await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.MovimientoOrigenId == movimientoSalida.Id ||
                        m.MovimientoOrigenId == movimientoEntrada.Id);

                if (reversionRegistradaDuranteLaOperacion)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] = "Los movimientos de la transferencia ya fueron revertidos por otra operación.";

                    return RedirectToAction(nameof(Details), new { id = transferencia.Id });
                }

                decimal saldoDisponibleDestino = await _context.MovimientosCaja
                    .AsNoTracking()
                    .Where(m =>
                        m.CajaId == transferencia.CajaDestinoId &&
                        m.EmpresaId == transferencia.EmpresaId)
                    .SumAsync(m =>
                        m.Direccion == DireccionMovimientoCaja.Ingreso
                            ? m.Importe
                            : -m.Importe);

                if (transferencia.Importe > saldoDisponibleDestino)
                {
                    await transaction.RollbackAsync();

                    ViewBag.Error = $"No se puede anular la transferencia porque la caja destino no tiene saldo suficiente. Disponible: {saldoDisponibleDestino:C}.";

                    return View(CrearResumenTransferencia(transferencia));
                }

                DateTime fecha =
                    DateTime.Now;

                // Revierte la salida original:
                // el dinero vuelve a Caja Origen.
                var reversionSalida =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            transferencia.EmpresaId,

                        CajaId =
                            transferencia.CajaOrigenId,

                        Tipo =
                            TipoMovimientoCaja
                                .ReversionTransferenciaSalida,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            transferencia.Importe,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            null,

                        TurnoCajaId =
                            transferencia.TurnoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Anulación transferencia #{transferencia.Id}",

                        Observaciones =
                            motivoAnulacion,

                        TransferenciaCajaId =
                            transferencia.Id,

                        MovimientoOrigenId =
                            movimientoSalida.Id
                    };

                // Revierte la entrada original:
                // el dinero sale nuevamente de Caja Destino.
                var reversionEntrada =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            transferencia.EmpresaId,

                        CajaId =
                            transferencia.CajaDestinoId,

                        Tipo =
                            TipoMovimientoCaja
                                .ReversionTransferenciaEntrada,

                        Direccion =
                            DireccionMovimientoCaja.Egreso,

                        Importe =
                            transferencia.Importe,

                        Fecha =
                            fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            null,

                        TurnoCajaId =
                            null,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Anulación transferencia #{transferencia.Id}",

                        Observaciones =
                            motivoAnulacion,

                        TransferenciaCajaId =
                            transferencia.Id,

                        MovimientoOrigenId =
                            movimientoEntrada.Id
                    };

                _context.MovimientosCaja.AddRange(
                    reversionSalida,
                    reversionEntrada);

                transferencia.Estado =
                    EstadoTransferenciaCaja.Anulada;

                transferencia.FechaAnulacion =
                    fecha;

                transferencia.UsuarioAnulacionId =
                    usuario.Id;

                transferencia.MotivoAnulacion =
                    motivoAnulacion;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Transferencia anulada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = transferencia.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ViewBag.Error =
                    "Ocurrió un error al anular la transferencia.";

                return View(
                    CrearResumenTransferencia(
                        transferencia));
            }
        }

        // Helpers
        private async Task CargarOpcionesTransferencia(TransferenciaCajaCreateVM vm, int empresaId)
        {
            var cajas =
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

            vm.CajasOrigenDisponibles =
                cajas;

            vm.CajasDestinoDisponibles =
                cajas;
        }
        private TransferenciaCajaResumenVM CrearResumenTransferencia(TransferenciaCaja transferencia)
        {
            return new TransferenciaCajaResumenVM
            {
                Id = transferencia.Id,
                Fecha = transferencia.Fecha,

                CajaOrigenNombre =
                    transferencia.CajaOrigen.Nombre,

                CajaDestinoNombre =
                    transferencia.CajaDestino.Nombre,

                Importe =
                    transferencia.Importe,

                Motivo =
                    transferencia.Motivo,

                UsuarioNombre =
                    transferencia.Usuario.UserName ?? "",

                TurnoCajaId =
                    transferencia.TurnoCajaId,

                Estado =
                    transferencia.Estado,

                FechaAnulacion =
                    transferencia.FechaAnulacion,

                UsuarioAnulacionNombre =
                    transferencia.UsuarioAnulacion?.UserName,

                MotivoAnulacion =
                    transferencia.MotivoAnulacion
            };
        }
    }
}
