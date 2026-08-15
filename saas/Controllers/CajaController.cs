using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;
using saas.Helpers;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class CajaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public CajaController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Caja
        public async Task<IActionResult> Index(CajaIndexVM cajaVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas
                .AsNoTracking()
                .Include(c => c.Empresa);

            // Seguridad multiempresa
            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(c => c.EmpresaId == usuario.EmpresaId);

                cajaVM.EmpresaId = null;
            }
            else if (cajaVM.EmpresaId.HasValue)
            {
                consulta = consulta
                    .Where(c => c.EmpresaId == cajaVM.EmpresaId.Value);
            }

            // Estado
            switch (cajaVM.Estado?.ToLower())
            {
                case "inactivas":
                    consulta = consulta.Where(c => !c.Estado);
                    break;

                case "todas":
                    break;

                default:
                    consulta = consulta.Where(c => c.Estado);
                    cajaVM.Estado = "activas";
                    break;
            }

            // Tipo
            if (cajaVM.Tipo.HasValue)
            {
                consulta = consulta
                    .Where(c => c.Tipo == cajaVM.Tipo.Value);
            }

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(cajaVM.Busqueda))
            {
                string busqueda = cajaVM.Busqueda.Trim();

                consulta = consulta
                    .Where(c => c.Nombre.Contains(busqueda));
            }

            cajaVM.Cajas = await consulta
                .OrderBy(c => c.Nombre)
                .Select(c => new CajaIndexItemVM
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    PermiteTurnos = c.PermiteTurnos,
                    FondoFijo = c.FondoFijo,
                    Estado = c.Estado,
                    FechaAlta = c.FechaAlta,
                    EmpresaNombre = c.Empresa.Nombre,

                    SaldoActual = _context.MovimientosCaja
                        .Where(m => m.CajaId == c.Id)
                        .Sum(m =>
                            m.Direccion ==
                            Models.Enums.DireccionMovimientoCaja.Ingreso
                                ? m.Importe
                                : -m.Importe)
                })
                .ToListAsync();

            // Empresas únicamente para SuperAdmin
            if (esSuperAdmin)
            {
                cajaVM.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
                    .ToListAsync();
            }

            return View(cajaVM);
        }
        // GET: Caja/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            var cajaVM = new CajaCreateVM();

            if (esSuperAdmin)
            {
                await CargarEmpresas(cajaVM);
            }
            else
            {
                cajaVM.EmpresaId = usuario.EmpresaId;

                await CargarMediosPago(
                    cajaVM,
                    usuario.EmpresaId);
            }

            return View(cajaVM);
        }
        // POST: Caja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CajaCreateVM cajaVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                cajaVM.EmpresaId = usuario.EmpresaId;
                ModelState.Remove(nameof(cajaVM.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                await RecargarCreate(cajaVM, esSuperAdmin);
                return View(cajaVM);
            }

            if (!cajaVM.EmpresaId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.EmpresaId),
                    "Debe seleccionar una empresa.");

                await RecargarCreate(cajaVM, esSuperAdmin);
                return View(cajaVM);
            }

            int empresaId = cajaVM.EmpresaId.Value;

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == empresaId &&
                    e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.EmpresaId),
                    "La empresa seleccionada no es válida.");

                await RecargarCreate(cajaVM, esSuperAdmin);
                return View(cajaVM);
            }

            // Seguridad adicional para AdminEmpresa
            if (!esSuperAdmin &&
                empresaId != usuario.EmpresaId)
            {
                return Forbid();
            }

            cajaVM.Nombre = cajaVM.Nombre.Trim();

            bool nombreDuplicado = await _context.Cajas
                .AsNoTracking()
                .AnyAsync(c =>
                    c.EmpresaId == empresaId &&
                    c.Nombre == cajaVM.Nombre &&
                    c.Estado);

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.Nombre),
                    "Ya existe una caja activa con ese nombre.");

                await RecargarCreate(cajaVM, esSuperAdmin);
                return View(cajaVM);
            }

            if (!Enum.IsDefined(cajaVM.Tipo))
            {
                ModelState.AddModelError(
                    nameof(cajaVM.Tipo),
                    "El tipo de caja seleccionado no es válido.");
            }

            if (cajaVM.PermiteTurnos &&
                cajaVM.Tipo != Models.Enums.TipoCaja.Efectivo)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.PermiteTurnos),
                    "Solo una caja de tipo Efectivo puede permitir turnos.");
            }

            if (!ModelState.IsValid)
            {
                await RecargarCreate(cajaVM, esSuperAdmin);
                return View(cajaVM);
            }

            var mediosIds = cajaVM.MediosPagoSeleccionadosIds
                .Distinct()
                .ToList();

            if (mediosIds.Count > 0)
            {
                var mediosSeleccionados = await _context.MediosPago
                    .AsNoTracking()
                    .Where(m =>
                        mediosIds.Contains(m.Id) &&
                        m.EmpresaId == empresaId &&
                        m.Estado)
                    .ToListAsync();

                // Validamos que todos los IDs enviados sean válidos
                if (mediosSeleccionados.Count != mediosIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(cajaVM.MediosPagoSeleccionadosIds),
                        "Uno o más medios de pago seleccionados no son válidos.");
                }
                else
                {
                    // Validamos compatibilidad entre Caja y MedioPago
                    var medioIncompatible = mediosSeleccionados
                        .FirstOrDefault(m =>
                            !CompatibilidadFinanciera.EsCompatible(
                                cajaVM.Tipo,
                                m.Tipo));

                    if (medioIncompatible != null)
                    {
                        ModelState.AddModelError(
                            nameof(cajaVM.MediosPagoSeleccionadosIds),
                            $"El medio de pago '{medioIncompatible.Nombre}' no es compatible con una caja de tipo {cajaVM.Tipo}.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await RecargarCreate(cajaVM, esSuperAdmin);
                    return View(cajaVM);
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var caja = new Caja
                {
                    Nombre = cajaVM.Nombre,
                    Tipo = cajaVM.Tipo,
                    PermiteTurnos = cajaVM.PermiteTurnos,
                    FondoFijo = cajaVM.FondoFijo,
                    Estado = true,
                    FechaAlta = DateTime.Now,
                    EmpresaId = empresaId
                };

                _context.Cajas.Add(caja);
                await _context.SaveChangesAsync();

                foreach (int medioPagoId in mediosIds)
                {
                    _context.CajaMediosPago.Add(
                        new CajaMedioPago
                        {
                            CajaId = caja.Id,
                            MedioPagoId = medioPagoId
                        });
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] = "La caja se creó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetMediosPagoPorEmpresa(
            int empresaId,
            Models.Enums.TipoCaja tipoCaja)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin &&
                usuario.EmpresaId != empresaId)
            {
                return Forbid();
            }

            if (!Enum.IsDefined(tipoCaja))
            {
                return BadRequest();
            }

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == empresaId &&
                    e.Estado);

            if (!empresaValida)
            {
                return BadRequest();
            }

            var medios = await _context.MediosPago
                .AsNoTracking()
                .Where(m =>
                    m.EmpresaId == empresaId &&
                    m.Estado)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            var compatibles = medios
                .Where(m =>
                    CompatibilidadFinanciera.EsCompatible(
                        tipoCaja,
                        m.Tipo))
                .Select(m => new
                {
                    id = m.Id,
                    nombre = m.Nombre
                })
                .ToList();

            return Json(compatibles);
        }
        // GET: Caja/Details/5
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
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas
                .AsNoTracking()
                .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var caja = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            decimal saldoActual = await CalcularSaldoCaja(caja.Id);

            var mediosPago = await _context.CajaMediosPago
                .AsNoTracking()
                .Where(cm => cm.CajaId == caja.Id)
                .OrderBy(cm => cm.MedioPago.Nombre)
                .Select(cm => cm.MedioPago.Nombre)
                .ToListAsync();

            bool tieneTurnoAbierto = await _context.TurnosCaja
                .AsNoTracking()
                .AnyAsync(t =>
                    t.CajaId == caja.Id &&
                    t.Estado == Models.Enums.EstadoTurnoCaja.Abierto);

            var cajaVM = new CajaDetailsVM
            {
                Id = caja.Id,
                Nombre = caja.Nombre,
                Tipo = caja.Tipo,
                PermiteTurnos = caja.PermiteTurnos,
                FondoFijo = caja.FondoFijo,
                Estado = caja.Estado,
                FechaAlta = caja.FechaAlta,
                EmpresaNombre = caja.Empresa.Nombre,
                SaldoActual = saldoActual,
                MediosPago = mediosPago,
                TieneTurnoAbierto = tieneTurnoAbierto
            };

            return View(cajaVM);
        }
        // GET: Caja/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
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
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var caja = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            var mediosSeleccionados = await _context.CajaMediosPago
                .AsNoTracking()
                .Where(cm => cm.CajaId == caja.Id)
                .Select(cm => cm.MedioPagoId)
                .ToListAsync();

            var cajaVM = new CajaEditVM
            {
                Id = caja.Id,
                Nombre = caja.Nombre,
                Tipo = caja.Tipo,
                PermiteTurnos = caja.PermiteTurnos,
                FondoFijo = caja.FondoFijo,
                Estado = caja.Estado,
                EmpresaId = caja.EmpresaId,
                MediosPagoSeleccionadosIds = mediosSeleccionados
            };

            await CargarMediosPago(cajaVM, caja.EmpresaId);

            return View(cajaVM);
        }
        // POST: Caja/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CajaEditVM cajaVM)
        {
            if (id != cajaVM.Id)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var caja = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            // Empresa nunca se cambia desde Edit.
            cajaVM.EmpresaId = caja.EmpresaId;
            ModelState.Remove(nameof(cajaVM.EmpresaId));

            if (!Enum.IsDefined(cajaVM.Tipo))
            {
                ModelState.AddModelError(
                    nameof(cajaVM.Tipo),
                    "El tipo de caja seleccionado no es válido.");
            }

            if (cajaVM.PermiteTurnos &&
                cajaVM.Tipo != Models.Enums.TipoCaja.Efectivo)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.PermiteTurnos),
                    "Solo una caja de tipo Efectivo puede permitir turnos.");
            }

            if (!ModelState.IsValid)
            {
                await CargarMediosPago(cajaVM, caja.EmpresaId);
                return View(cajaVM);
            }

            cajaVM.Nombre = cajaVM.Nombre.Trim();

            bool nombreDuplicado = await _context.Cajas
                .AsNoTracking()
                .AnyAsync(c =>
                    c.Id != caja.Id &&
                    c.EmpresaId == caja.EmpresaId &&
                    c.Nombre.ToLower() ==
                        cajaVM.Nombre.ToLower());

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(cajaVM.Nombre),
                    "Ya existe otra caja con ese nombre para esta empresa.");

                await CargarMediosPago(cajaVM, caja.EmpresaId);
                return View(cajaVM);
            }

            bool tieneTurnoAbierto = await _context.TurnosCaja
                .AsNoTracking()
                .AnyAsync(t =>
                    t.CajaId == caja.Id &&
                    t.Estado == Models.Enums.EstadoTurnoCaja.Abierto);

            // No permitimos quitar capacidad de turnos
            // mientras exista uno abierto.
            if (tieneTurnoAbierto &&
                (!cajaVM.PermiteTurnos ||
                 cajaVM.Tipo != Models.Enums.TipoCaja.Efectivo))
            {
                ModelState.AddModelError(
                    nameof(cajaVM.PermiteTurnos),
                    "No puede modificar esta configuración mientras la caja tenga un turno abierto.");

                await CargarMediosPago(cajaVM, caja.EmpresaId);
                return View(cajaVM);
            }

            // Si se intenta desactivar desde Edit,
            // aplicamos las mismas reglas que en Delete.
            if (caja.Estado && !cajaVM.Estado)
            {
                if (tieneTurnoAbierto)
                {
                    ModelState.AddModelError(
                        nameof(cajaVM.Estado),
                        "No puede desactivar una caja con un turno abierto.");

                    await CargarMediosPago(cajaVM, caja.EmpresaId);
                    return View(cajaVM);
                }

                decimal saldoActual = await CalcularSaldoCaja(caja.Id);

                if (saldoActual != 0)
                {
                    ModelState.AddModelError(
                        nameof(cajaVM.Estado),
                        "No puede desactivar una caja mientras tenga saldo. Primero debe dejar su saldo en cero.");

                    await CargarMediosPago(cajaVM, caja.EmpresaId);
                    return View(cajaVM);
                }
            }

            var mediosIds = cajaVM.MediosPagoSeleccionadosIds
                .Distinct()
                .ToList();

            if (mediosIds.Count > 0)
            {
                var mediosSeleccionados = await _context.MediosPago
                    .AsNoTracking()
                    .Where(m =>
                        mediosIds.Contains(m.Id) &&
                        m.EmpresaId == caja.EmpresaId &&
                        m.Estado)
                    .ToListAsync();

                if (mediosSeleccionados.Count != mediosIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(cajaVM.MediosPagoSeleccionadosIds),
                        "Uno o más medios de pago seleccionados no son válidos.");
                }
                else
                {
                    var medioIncompatible = mediosSeleccionados
                        .FirstOrDefault(m =>
                            !CompatibilidadFinanciera.EsCompatible(
                                cajaVM.Tipo,
                                m.Tipo));

                    if (medioIncompatible != null)
                    {
                        ModelState.AddModelError(
                            nameof(cajaVM.MediosPagoSeleccionadosIds),
                            $"El medio de pago '{medioIncompatible.Nombre}' no es compatible con una caja de tipo {cajaVM.Tipo}.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await CargarMediosPago(cajaVM, caja.EmpresaId);
                    return View(cajaVM);
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                caja.Nombre = cajaVM.Nombre;
                caja.Tipo = cajaVM.Tipo;
                caja.PermiteTurnos = cajaVM.PermiteTurnos;
                caja.FondoFijo = cajaVM.FondoFijo;
                caja.Estado = cajaVM.Estado;

                var relacionesActuales = await _context.CajaMediosPago
                    .Where(cm => cm.CajaId == caja.Id)
                    .ToListAsync();

                var actualesIds = relacionesActuales
                    .Select(cm => cm.MedioPagoId)
                    .ToHashSet();

                var seleccionadosIds = mediosIds.ToHashSet();

                // Eliminar asociaciones que dejaron de seleccionarse
                var relacionesEliminar = relacionesActuales
                    .Where(cm =>
                        !seleccionadosIds.Contains(cm.MedioPagoId))
                    .ToList();

                _context.CajaMediosPago
                    .RemoveRange(relacionesEliminar);

                // Agregar las nuevas
                var nuevosIds = seleccionadosIds
                    .Except(actualesIds);

                foreach (int medioPagoId in nuevosIds)
                {
                    _context.CajaMediosPago.Add(
                        new CajaMedioPago
                        {
                            CajaId = caja.Id,
                            MedioPagoId = medioPagoId
                        });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    caja.Estado
                        ? "Caja modificada correctamente."
                        : "Caja modificada y desactivada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar la caja.");

                await CargarMediosPago(cajaVM, caja.EmpresaId);

                return View(cajaVM);
            }
        }
        // GET: Caja/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
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
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas
                .AsNoTracking()
                .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var caja = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            if (!caja.Estado)
            {
                TempData["Error"] =
                    "La caja ya se encuentra inactiva.";

                return RedirectToAction(nameof(Index));
            }

            var cajaVM = new CajaDetailsVM
            {
                Id = caja.Id,
                Nombre = caja.Nombre,
                Tipo = caja.Tipo,
                PermiteTurnos = caja.PermiteTurnos,
                FondoFijo = caja.FondoFijo,
                Estado = caja.Estado,
                FechaAlta = caja.FechaAlta,
                EmpresaNombre = caja.Empresa.Nombre,
                SaldoActual = await CalcularSaldoCaja(caja.Id),
                TieneTurnoAbierto = await _context.TurnosCaja
                    .AsNoTracking()
                    .AnyAsync(t =>
                        t.CajaId == caja.Id &&
                        t.Estado ==
                            Models.Enums.EstadoTurnoCaja.Abierto),

                MediosPago = await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm => cm.CajaId == caja.Id)
                    .OrderBy(cm => cm.MedioPago.Nombre)
                    .Select(cm => cm.MedioPago.Nombre)
                    .ToListAsync()
            };

            return View(cajaVM);
        }
        // POST: Caja/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Caja> consulta = _context.Cajas;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var caja = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            if (!caja.Estado)
            {
                TempData["Error"] =
                    "La caja ya se encuentra inactiva.";

                return RedirectToAction(nameof(Index));
            }

            bool tieneTurnoAbierto = await _context.TurnosCaja
                .AsNoTracking()
                .AnyAsync(t =>
                    t.CajaId == caja.Id &&
                    t.Estado ==
                        Models.Enums.EstadoTurnoCaja.Abierto);

            if (tieneTurnoAbierto)
            {
                TempData["Error"] =
                    "No puede desactivar una caja con un turno abierto.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }

            decimal saldoActual =
                await CalcularSaldoCaja(caja.Id);

            if (saldoActual != 0)
            {
                TempData["Error"] =
                    "No puede desactivar una caja mientras tenga saldo. Primero debe dejar su saldo en cero.";

                return RedirectToAction(
                    nameof(Delete),
                    new { id });
            }

            try
            {
                caja.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Caja desactivada correctamente.";
            }
            catch
            {
                TempData["Error"] =
                    "Ocurrió un error al desactivar la caja.";
            }

            return RedirectToAction(nameof(Index));
        }





        //Helper methods
        private async Task CargarEmpresas(CajaCreateVM cajaVM)
        {
            cajaVM.Empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre
                })
                .ToListAsync();
        }
        private async Task CargarMediosPago(CajaCreateVM cajaVM, int? empresaId)
        {
            cajaVM.MediosPagoDisponibles.Clear();

            if (!empresaId.HasValue)
            {
                return;
            }

            var medios = await _context.MediosPago
                .AsNoTracking()
                .Where(m =>
                    m.EmpresaId == empresaId.Value &&
                    m.Estado)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            cajaVM.MediosPagoDisponibles = medios
                .Where(m =>
                    CompatibilidadFinanciera.EsCompatible(
                        cajaVM.Tipo,
                        m.Tipo))
                .Select(m => new MedioPagoOpcionVM
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Seleccionado =
                        cajaVM.MediosPagoSeleccionadosIds.Contains(m.Id)
                })
                .ToList();
        }
        private async Task RecargarCreate(CajaCreateVM cajaVM, bool esSuperAdmin)
        {
            if (esSuperAdmin)
            {
                await CargarEmpresas(cajaVM);
            }

            await CargarMediosPago(
                cajaVM,
                cajaVM.EmpresaId);
        }
        private async Task CargarMediosPago(CajaEditVM cajaVM, int empresaId)
        {
            var medios = await _context.MediosPago
                .AsNoTracking()
                .Where(m =>
                    m.EmpresaId == empresaId &&
                    m.Estado)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            cajaVM.MediosPagoDisponibles = medios
                .Where(m =>
                    CompatibilidadFinanciera.EsCompatible(
                        cajaVM.Tipo,
                        m.Tipo))
                .Select(m => new MedioPagoOpcionVM
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Seleccionado =
                        cajaVM.MediosPagoSeleccionadosIds.Contains(m.Id)
                })
                .ToList();
        }
        private async Task<decimal> CalcularSaldoCaja(int cajaId)
        {
            return await _context.MovimientosCaja
                .AsNoTracking()
                .Where(m => m.CajaId == cajaId)
                .SumAsync(m =>
                    m.Direccion ==
                    Models.Enums.DireccionMovimientoCaja.Ingreso
                        ? m.Importe
                        : -m.Importe);
        }

    }
}