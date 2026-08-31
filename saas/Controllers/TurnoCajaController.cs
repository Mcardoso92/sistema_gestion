using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class TurnoCajaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public TurnoCajaController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TurnoCaja
        public async Task<IActionResult> Index(string estado = "abiertos", int? empresaId = null, string? busqueda = null, int pagina = 1)
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

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .AsNoTracking()
                    .Include(t => t.Empresa)
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura);

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

            switch (estado.ToLower())
            {
                case "cerrados":
                    consulta = consulta.Where(t =>
                        t.Estado == EstadoTurnoCaja.Cerrado);
                    break;

                case "todos":
                    break;

                default:
                    consulta = consulta.Where(t =>
                        t.Estado == EstadoTurnoCaja.Abierto);

                    estado = "abiertos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                consulta = consulta.Where(t =>
                    t.Caja.Nombre.Contains(busqueda) ||
                    t.UsuarioApertura.UserName!.Contains(busqueda));
            }

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalTurnos = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalTurnos / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalTurnos;

            var turnos = await consulta
                .OrderByDescending(t => t.FechaApertura)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(t => new TurnoCajaIndexVM
                {
                    Id = t.Id,
                    CajaNombre = t.Caja.Nombre,
                    UsuarioAperturaNombre =
                        t.UsuarioApertura.UserName ?? "",
                    FechaApertura = t.FechaApertura,
                    FechaCierre = t.FechaCierre,
                    Estado = t.Estado,
                    CierreForzado = t.CierreForzado,
                    FondoFijoAplicado = t.FondoFijoAplicado,
                    EfectivoEsperado = t.EfectivoEsperado,
                    EfectivoContado = t.EfectivoContado,
                    Diferencia = t.Diferencia,
                    EmpresaNombre = t.Empresa.Nombre
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                ViewBag.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
            }

            ViewBag.Estado = estado;
            ViewBag.EmpresaId =
                esSuperAdmin ? empresaId : null;
            ViewBag.Busqueda = busqueda;

            return View(turnos);
        }

        // GET: TurnoCaja/Abrir
        [HttpGet]
        public async Task<IActionResult> Abrir()
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

            int? empresaId = usuario.EmpresaId;

            if (esSuperAdmin)
            {
                TempData["Error"] =
                    "Para abrir un turno como SuperAdmin debe operar dentro de una empresa específica.";

                return RedirectToAction(nameof(Index));
            }

            bool usuarioTieneTurnoAbierto =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .AnyAsync(t =>
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

            if (usuarioTieneTurnoAbierto)
            {
                TempData["Error"] =
                    "Ya tiene un turno de caja abierto.";

                return RedirectToAction(nameof(Index));
            }

            var cajaIdsOcupadas =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .Where(t =>
                        t.EmpresaId == empresaId &&
                        t.Estado == EstadoTurnoCaja.Abierto)
                    .Select(t => t.CajaId)
                    .ToListAsync();

            var cajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado &&
                        c.Tipo == TipoCaja.Efectivo &&
                        c.PermiteTurnos &&
                        !cajaIdsOcupadas.Contains(c.Id))
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CajaTurnoOpcionVM
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        FondoFijo = c.FondoFijo,
                        Disponible = true
                    })
                    .ToListAsync();

            var vm = new AperturaTurnoVM
            {
                CajasDisponibles = cajasDisponibles
            };

            return View(vm);
        }

        // POST: TurnoCaja/Abrir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(AperturaTurnoVM vm)
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

            if (esSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            bool usuarioTieneTurnoAbierto =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .AnyAsync(t =>
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

            if (usuarioTieneTurnoAbierto)
            {
                ModelState.AddModelError(
                    "",
                    "Ya tiene un turno de caja abierto.");

                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            var caja = await _context.Cajas
                .FirstOrDefaultAsync(c =>
                    c.Id == vm.CajaId &&
                    c.EmpresaId == usuario.EmpresaId &&
                    c.Estado);

            if (caja == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no es válida.");

                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            if (caja.Tipo != TipoCaja.Efectivo ||
                !caja.PermiteTurnos)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no permite turnos.");

                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            bool cajaTieneTurnoAbierto =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .AnyAsync(t =>
                        t.CajaId == caja.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

            if (cajaTieneTurnoAbierto)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada ya tiene un turno abierto.");

                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                // Revalidamos dentro de SERIALIZABLE para evitar
                // aperturas simultáneas sobre el mismo usuario o caja.

                bool usuarioTieneTurnoAbiertoActual =
                    await _context.TurnosCaja
                        .AnyAsync(t =>
                            t.UsuarioAperturaId == usuario.Id &&
                            t.Estado == EstadoTurnoCaja.Abierto);

                if (usuarioTieneTurnoAbiertoActual)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "Ya tiene un turno de caja abierto.");

                    await RecargarCajasDisponibles(
                        vm,
                        usuario.EmpresaId);

                    return View(vm);
                }

                bool cajaTieneTurnoAbiertoActual =
                    await _context.TurnosCaja
                        .AnyAsync(t =>
                            t.CajaId == caja.Id &&
                            t.Estado == EstadoTurnoCaja.Abierto);

                if (cajaTieneTurnoAbiertoActual)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        "La caja seleccionada acaba de ser ocupada por otro turno.");

                    await RecargarCajasDisponibles(
                        vm,
                        usuario.EmpresaId);

                    return View(vm);
                }

                var turno = new TurnoCaja
                {
                    EmpresaId = caja.EmpresaId,
                    CajaId = caja.Id,
                    UsuarioAperturaId = usuario.Id,
                    FechaApertura = DateTime.Now,
                    Estado = EstadoTurnoCaja.Abierto,

                    FondoFijoAplicado = caja.FondoFijo,

                    FechaCierre = null,
                    UsuarioCierreId = null,
                    CierreForzado = false,
                    MotivoCierreForzado = null,
                    EfectivoEsperado = null,
                    EfectivoContado = null,
                    Diferencia = null,
                    ImporteRendido = null
                };

                _context.TurnosCaja.Add(turno);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Turno abierto correctamente en {caja.Nombre}.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al abrir el turno de caja.");

                await RecargarCajasDisponibles(
                    vm,
                    usuario.EmpresaId);

                return View(vm);
            }
        }
        // GET: TurnoCaja/Details/5
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

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .AsNoTracking()
                    .Include(t => t.Empresa)
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura)
                    .Include(t => t.UsuarioCierre);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var turno = await consulta
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound();
            }

            var movimientos = await _context.MovimientosCaja
                .AsNoTracking()
                .Where(m => m.TurnoCajaId == turno.Id)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new TurnoMovimientoResumenVM
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Direccion = m.Direccion,
                    CajaNombre = m.Caja.Nombre,
                    MedioPagoNombre =
                        m.MedioPago != null
                            ? m.MedioPago.Nombre
                            : null,
                    Concepto = m.Concepto,
                    Importe = m.Importe
                })
                .ToListAsync();

            var cobrosPorMedioPago =
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.TurnoCajaId == turno.Id &&
                        c.Estado == EstadoCobro.Activo)
                    .GroupBy(c => new
                    {
                        c.MedioPagoId,
                        c.MedioPago.Nombre
                    })
                    .Select(g =>
                        new TurnoCobroMedioPagoResumenVM
                        {
                            MedioPagoId =
                                g.Key.MedioPagoId,

                            MedioPagoNombre =
                                g.Key.Nombre,

                            Total =
                                g.Sum(c => c.Importe),

                            CantidadCobros =
                                g.Count()
                        })
                    .OrderByDescending(c =>
                        c.Total)
                    .ToListAsync();

            var movimientoRegularizacion =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .Include(m => m.Usuario)
                    .Where(m =>
                        m.TurnoCajaId == turno.Id &&
                        (
                            m.Tipo == TipoMovimientoCaja.AjusteSobranteCaja ||
                            m.Tipo == TipoMovimientoCaja.AjusteFaltanteCaja
                        ))
                    .OrderByDescending(m => m.Fecha)
                    .FirstOrDefaultAsync();

            var vm = new TurnoCajaDetailsVM
            {
                Id = turno.Id,
                EmpresaNombre = turno.Empresa.Nombre,
                CajaNombre = turno.Caja.Nombre,
                UsuarioAperturaNombre =
                    turno.UsuarioApertura.UserName ?? "",
                FechaApertura = turno.FechaApertura,
                Estado = turno.Estado,
                FondoFijoAplicado = turno.FondoFijoAplicado,

                FechaCierre = turno.FechaCierre,
                UsuarioCierreNombre =
                    turno.UsuarioCierre != null
                        ? turno.UsuarioCierre.UserName
                        : null,

                CierreForzado = turno.CierreForzado,
                MotivoCierreForzado =
                    turno.MotivoCierreForzado,

                EfectivoEsperado = turno.EfectivoEsperado,
                EfectivoContado = turno.EfectivoContado,
                Diferencia = turno.Diferencia,
                ImporteRendido = turno.ImporteRendido,

                Movimientos = movimientos,

                CobrosPorMedioPago = cobrosPorMedioPago,

                Regularizacion =
                new RegularizacionTurnoResumenVM
                {
                    Regularizado =
                        movimientoRegularizacion != null,

                    MovimientoCajaId =
                        movimientoRegularizacion?.Id,

                    FechaRegularizacion =
                        movimientoRegularizacion?.Fecha,

                    UsuarioRegularizacionNombre =
                        movimientoRegularizacion != null
                            ? movimientoRegularizacion.Usuario.UserName
                            : null,

                    Importe =
                        movimientoRegularizacion?.Importe,

                    Motivo =
                        movimientoRegularizacion?.Observaciones
                }
            };

            return View(vm);
        }
        // GET: TurnoCaja/Cerrar/5
        [HttpGet]
        public async Task<IActionResult> Cerrar(int? id)
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

            bool esAdminEmpresa =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "AdminEmpresa");

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .AsNoTracking()
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var turno = await consulta
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound();
            }

            if (turno.Estado != EstadoTurnoCaja.Abierto)
            {
                TempData["Error"] =
                    "El turno ya se encuentra cerrado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            bool esPropietarioTurno =
                turno.UsuarioAperturaId == usuario.Id;

            // El usuario normal solo puede cerrar su propio turno.
            // AdminEmpresa/SuperAdmin pueden forzar el cierre.
            if (!esPropietarioTurno &&
                !esAdminEmpresa &&
                !esSuperAdmin)
            {
                return Forbid();
            }

            decimal efectivoEsperado =
                await CalcularEfectivoEsperado(turno);

            var vm = new CierreTurnoVM
            {
                TurnoCajaId = turno.Id,
                CajaNombre = turno.Caja.Nombre,

                UsuarioAperturaNombre =
                    turno.UsuarioApertura.UserName ?? "",

                FechaApertura =
                    turno.FechaApertura,

                FondoFijoAplicado =
                    turno.FondoFijoAplicado,

                EfectivoEsperado =
                    efectivoEsperado,

                EfectivoContado =
                    efectivoEsperado,

                Diferencia = 0,

                ImporteRendirSugerido =
                    Math.Max(
                        0,
                        efectivoEsperado -
                        turno.FondoFijoAplicado),

                ImporteRendido =
                    Math.Max(
                        0,
                        efectivoEsperado -
                        turno.FondoFijoAplicado),

                CierreForzado =
                    !esPropietarioTurno
            };

            await CargarCajasDestino(vm, turno);

            return View(vm);
        }
        // POST: TurnoCaja/Cerrar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(int id, CierreTurnoVM vm)
        {
            if (id != vm.TurnoCajaId)
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

            bool esAdminEmpresa =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "AdminEmpresa");

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var turno = await consulta
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound();
            }

            if (turno.Estado != EstadoTurnoCaja.Abierto)
            {
                TempData["Error"] =
                    "El turno ya se encuentra cerrado.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            bool esPropietarioTurno =
                turno.UsuarioAperturaId == usuario.Id;

            bool cierreForzado =
                !esPropietarioTurno;

            if (cierreForzado &&
                !esAdminEmpresa &&
                !esSuperAdmin)
            {
                return Forbid();
            }

            // El servidor determina estos valores.
            vm.CierreForzado = cierreForzado;

            decimal efectivoEsperado =
                await CalcularEfectivoEsperado(turno);

            vm.EfectivoEsperado =
                efectivoEsperado;

            vm.Diferencia =
                vm.EfectivoContado -
                efectivoEsperado;

            vm.ImporteRendirSugerido =
                Math.Max(
                    0,
                    vm.EfectivoContado -
                    turno.FondoFijoAplicado);

            if (vm.ImporteRendido > 0)
            {
                if (!vm.CajaDestinoId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaDestinoId),
                        "Debe seleccionar la caja donde se rendirá el dinero.");
                }
                else
                {
                    bool cajaDestinoValida =
                        await _context.Cajas
                            .AsNoTracking()
                            .AnyAsync(c =>
                                c.Id == vm.CajaDestinoId.Value &&
                                c.EmpresaId == turno.EmpresaId &&
                                c.Id != turno.CajaId &&
                                c.Estado &&
                                c.Tipo == TipoCaja.Efectivo);

                    if (!cajaDestinoValida)
                    {
                        ModelState.AddModelError(
                            nameof(vm.CajaDestinoId),
                            "La caja destino seleccionada no es válida.");
                    }
                }
            }
            else
            {
                vm.CajaDestinoId = null;
            }

            vm.CajaNombre =
                turno.Caja.Nombre;

            vm.UsuarioAperturaNombre =
                turno.UsuarioApertura.UserName ?? "";

            vm.FechaApertura =
                turno.FechaApertura;

            vm.FondoFijoAplicado =
                turno.FondoFijoAplicado;

            if (cierreForzado)
            {
                if (string.IsNullOrWhiteSpace(
                    vm.MotivoCierreForzado))
                {
                    ModelState.AddModelError(
                        nameof(vm.MotivoCierreForzado),
                        "Debe indicar el motivo del cierre forzado.");
                }
                else
                {
                    vm.MotivoCierreForzado =
                        vm.MotivoCierreForzado.Trim();
                }
            }
            else
            {
                vm.MotivoCierreForzado = null;
            }

            if (vm.ImporteRendido >
                vm.EfectivoContado)
            {
                ModelState.AddModelError(
                    nameof(vm.ImporteRendido),
                    "El importe rendido no puede superar el efectivo contado.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCajasDestino(vm, turno);

                return View(vm);
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                // Revalidamos el turno dentro de la transacción.
                // Evita cierres simultáneos o movimientos ingresados
                // entre la carga de la pantalla y la confirmación.

                await _context.Entry(turno)
                    .ReloadAsync();

                if (turno.Estado !=
                    EstadoTurnoCaja.Abierto)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "El turno ya fue cerrado por otra operación.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = turno.Id });
                }

                if (vm.ImporteRendido > 0 && vm.CajaDestinoId.HasValue)
                {
                    bool cajaDestinoValidaActual = await _context.Cajas
                        .AsNoTracking()
                        .AnyAsync(c =>
                            c.Id == vm.CajaDestinoId.Value &&
                            c.EmpresaId == turno.EmpresaId &&
                            c.Id != turno.CajaId &&
                            c.Estado &&
                            c.Tipo == TipoCaja.Efectivo);

                    if (!cajaDestinoValidaActual)
                    {
                        await transaction.RollbackAsync();

                        ModelState.AddModelError(nameof(vm.CajaDestinoId), "La caja destino dejó de estar disponible.");

                        await CargarCajasDestino(vm, turno);

                        return View(vm);
                    }
                }

                decimal efectivoEsperadoActual =
                    await CalcularEfectivoEsperado(
                        turno);

                vm.EfectivoEsperado =
                    efectivoEsperadoActual;

                vm.Diferencia =
                    vm.EfectivoContado -
                    efectivoEsperadoActual;

                turno.FechaCierre = DateTime.Now;
                turno.UsuarioCierreId = usuario.Id;
                turno.Estado = EstadoTurnoCaja.Cerrado;
                turno.CierreForzado = cierreForzado;
                turno.MotivoCierreForzado = vm.MotivoCierreForzado;
                turno.EfectivoEsperado = efectivoEsperadoActual;
                turno.EfectivoContado = vm.EfectivoContado;
                turno.Diferencia = vm.Diferencia;
                turno.ImporteRendido = vm.ImporteRendido;

                if (vm.ImporteRendido > 0 &&
                    vm.CajaDestinoId.HasValue)
                {
                    var transferencia = new TransferenciaCaja
                    {
                        EmpresaId = turno.EmpresaId,
                        CajaOrigenId = turno.CajaId,
                        CajaDestinoId = vm.CajaDestinoId.Value,
                        UsuarioId = usuario.Id,
                        TurnoCajaId = turno.Id,
                        Fecha = DateTime.Now,
                        Importe = vm.ImporteRendido,
                        Motivo = $"Rendición de turno #{turno.Id}",
                        Estado = EstadoTransferenciaCaja.Activa
                    };

                    _context.TransferenciasCaja.Add(transferencia);

                    await _context.SaveChangesAsync();

                    var movimientoSalida = new MovimientoCaja
                    {
                        EmpresaId = turno.EmpresaId,
                        CajaId = turno.CajaId,
                        Tipo = TipoMovimientoCaja.TransferenciaSalida,
                        Direccion = DireccionMovimientoCaja.Egreso,
                        Importe = vm.ImporteRendido,
                        Fecha = transferencia.Fecha,
                        UsuarioId = usuario.Id,
                        MedioPagoId = null,
                        TurnoCajaId = turno.Id,
                        CategoriaGastoId = null,
                        Concepto = $"Rendición de turno #{turno.Id}",
                        Observaciones = null,
                        TransferenciaCajaId = transferencia.Id
                    };

                    var movimientoEntrada = new MovimientoCaja
                    {
                        EmpresaId = turno.EmpresaId,
                        CajaId = vm.CajaDestinoId.Value,
                        Tipo = TipoMovimientoCaja.TransferenciaEntrada,
                        Direccion = DireccionMovimientoCaja.Ingreso,
                        Importe = vm.ImporteRendido,
                        Fecha = transferencia.Fecha,
                        UsuarioId = usuario.Id,
                        MedioPagoId = null,
                        TurnoCajaId = null,
                        CategoriaGastoId = null,
                        Concepto = $"Rendición recibida del turno #{turno.Id}",
                        Observaciones = null,
                        TransferenciaCajaId = transferencia.Id
                    };

                    _context.MovimientosCaja.AddRange(
                        movimientoSalida,
                        movimientoEntrada);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    cierreForzado
                        ? "Turno cerrado de forma forzada correctamente."
                        : "Turno cerrado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al cerrar el turno.");

                await CargarCajasDestino(vm, turno);

                return View(vm);
            }
        }
        // GET: TurnoCaja/RegularizarDiferencia/5
        [HttpGet]
        public async Task<IActionResult> RegularizarDiferencia(int? id)
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

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .AsNoTracking()
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var turno = await consulta
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound();
            }

            if (turno.Estado != EstadoTurnoCaja.Cerrado)
            {
                TempData["Error"] =
                    "Solo pueden regularizarse turnos cerrados.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            if (!turno.Diferencia.HasValue ||
                turno.Diferencia.Value == 0)
            {
                TempData["Error"] =
                    "El turno no tiene diferencias pendientes de regularización.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            bool yaRegularizado =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.TurnoCajaId == turno.Id &&
                        (
                            m.Tipo == TipoMovimientoCaja.AjusteSobranteCaja ||
                            m.Tipo == TipoMovimientoCaja.AjusteFaltanteCaja
                        ));

            if (yaRegularizado)
            {
                TempData["Error"] =
                    "La diferencia de este turno ya fue regularizada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            var vm = new RegularizarDiferenciaTurnoVM
            {
                TurnoCajaId = turno.Id,
                CajaNombre = turno.Caja.Nombre,
                UsuarioTurnoNombre =
                    turno.UsuarioApertura.UserName ?? "",
                FechaCierre =
                    turno.FechaCierre!.Value,
                EfectivoEsperado =
                    turno.EfectivoEsperado!.Value,
                EfectivoContado =
                    turno.EfectivoContado!.Value,
                Diferencia =
                    turno.Diferencia.Value
            };

            return View(vm);
        }
        // POST: TurnoCaja/RegularizarDiferencia/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegularizarDiferencia(int id, RegularizarDiferenciaTurnoVM vm)
        {
            if (id != vm.TurnoCajaId)
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

            IQueryable<TurnoCaja> consulta =
                _context.TurnosCaja
                    .Include(t => t.Caja)
                    .Include(t => t.UsuarioApertura);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(t =>
                    t.EmpresaId == usuario.EmpresaId);
            }

            var turno = await consulta
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turno == null)
            {
                return NotFound();
            }

            if (turno.Estado != EstadoTurnoCaja.Cerrado)
            {
                TempData["Error"] =
                    "Solo pueden regularizarse turnos cerrados.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            if (!turno.Diferencia.HasValue ||
                turno.Diferencia.Value == 0)
            {
                TempData["Error"] =
                    "El turno no tiene diferencias pendientes de regularización.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            bool yaRegularizado =
                await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.TurnoCajaId == turno.Id &&
                        (
                            m.Tipo == TipoMovimientoCaja.AjusteSobranteCaja ||
                            m.Tipo == TipoMovimientoCaja.AjusteFaltanteCaja
                        ));

            if (yaRegularizado)
            {
                TempData["Error"] =
                    "La diferencia de este turno ya fue regularizada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }

            vm.CajaNombre =
                turno.Caja.Nombre;

            vm.UsuarioTurnoNombre =
                turno.UsuarioApertura.UserName ?? "";

            vm.FechaCierre =
                turno.FechaCierre!.Value;

            vm.EfectivoEsperado =
                turno.EfectivoEsperado!.Value;

            vm.EfectivoContado =
                turno.EfectivoContado!.Value;

            vm.Diferencia =
                turno.Diferencia.Value;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            vm.Motivo =
                vm.Motivo.Trim();

            bool esSobrante =
                turno.Diferencia.Value > 0;

            var tipo =
                esSobrante
                    ? TipoMovimientoCaja.AjusteSobranteCaja
                    : TipoMovimientoCaja.AjusteFaltanteCaja;

            var direccion =
                esSobrante
                    ? DireccionMovimientoCaja.Ingreso
                    : DireccionMovimientoCaja.Egreso;

            decimal importe =
                Math.Abs(turno.Diferencia.Value);

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                bool yaRegularizadoActual =
                    await _context.MovimientosCaja
                        .AnyAsync(m =>
                            m.TurnoCajaId == turno.Id &&
                            (
                                m.Tipo == TipoMovimientoCaja.AjusteSobranteCaja ||
                                m.Tipo == TipoMovimientoCaja.AjusteFaltanteCaja
                            ));

                if (yaRegularizadoActual)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "La diferencia de este turno ya fue regularizada por otra operación.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = turno.Id });
                }

                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            turno.EmpresaId,

                        CajaId =
                            turno.CajaId,

                        Tipo =
                            tipo,

                        Direccion =
                            direccion,

                        Importe =
                            importe,

                        Fecha =
                            DateTime.Now,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            null,

                        TurnoCajaId =
                            turno.Id,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            esSobrante
                                ? $"Regularización de sobrante del turno #{turno.Id}"
                                : $"Regularización de faltante del turno #{turno.Id}",

                        Observaciones =
                            vm.Motivo
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Diferencia regularizada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = turno.Id });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al regularizar la diferencia.");

                return View(vm);
            }
        }

        //Helpers Methods
        private async Task RecargarCajasDisponibles(AperturaTurnoVM vm, int empresaId)
        {
            var cajasOcupadas =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .Where(t =>
                        t.EmpresaId == empresaId &&
                        t.Estado == EstadoTurnoCaja.Abierto)
                    .Select(t => t.CajaId)
                    .ToListAsync();

            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado &&
                        c.Tipo == TipoCaja.Efectivo &&
                        c.PermiteTurnos &&
                        !cajasOcupadas.Contains(c.Id))
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CajaTurnoOpcionVM
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        FondoFijo = c.FondoFijo,
                        Disponible = true
                    })
                    .ToListAsync();
        }
        private async Task<decimal> CalcularEfectivoEsperado(TurnoCaja turno)
        {
            decimal netoMovimientos =
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
                + netoMovimientos;
        }
        private async Task CargarCajasDestino(CierreTurnoVM vm, TurnoCaja turno)
        {
            vm.CajasDestinoDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == turno.EmpresaId &&
                        c.Id != turno.CajaId &&
                        c.Estado &&
                        c.Tipo == TipoCaja.Efectivo)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CajaTurnoOpcionVM
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        FondoFijo = c.FondoFijo,
                        Disponible = true
                    })
                    .ToListAsync();
        }

    }
}
