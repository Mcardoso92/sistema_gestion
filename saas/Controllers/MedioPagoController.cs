using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class MedioPagoController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public MedioPagoController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MedioPago
        public async Task<IActionResult> Index(
            string estado = "activos",
            int? empresaId = null,
            string? busqueda = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<MedioPago> consulta = _context.MediosPago
                .AsNoTracking()
                .Include(m => m.Empresa);

            // Seguridad multiempresa
            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);

                empresaId = null;
            }
            else if (empresaId.HasValue)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == empresaId.Value);
            }

            // Estado
            switch (estado.ToLower())
            {
                case "inactivos":
                    consulta = consulta.Where(m => !m.Estado);
                    break;

                case "todos":
                    break;

                default:
                    consulta = consulta.Where(m => m.Estado);
                    estado = "activos";
                    break;
            }

            // Búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                consulta = consulta.Where(m =>
                    m.Nombre.Contains(busqueda) ||
                    (m.Descripcion != null &&
                     m.Descripcion.Contains(busqueda)));
            }

            var mediosPago = await consulta
                .OrderBy(m => m.Nombre)
                .Select(m => new MedioPagoIndexVM
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,
                    Tipo = m.Tipo,
                    Estado = m.Estado,
                    FechaAlta = m.FechaAlta,
                    EmpresaNombre = m.Empresa.Nombre
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
            ViewBag.EmpresaId = esSuperAdmin ? empresaId : null;
            ViewBag.Busqueda = busqueda;

            return View(mediosPago);
        }

        // GET: MedioPago/Create
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

            var medioPagoVM = new MedioPagoCreateVM();

            if (esSuperAdmin)
            {
                await CargarEmpresas();
            }
            else
            {
                medioPagoVM.EmpresaId = usuario.EmpresaId;
            }

            return View(medioPagoVM);
        }

        // POST: MedioPago/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MedioPagoCreateVM medioPagoVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            // AdminEmpresa nunca decide la empresa desde el POST
            if (!esSuperAdmin)
            {
                medioPagoVM.EmpresaId = usuario.EmpresaId;
                ModelState.Remove(nameof(medioPagoVM.EmpresaId));
            }

            if (!Enum.IsDefined(medioPagoVM.Tipo))
            {
                ModelState.AddModelError(
                    nameof(medioPagoVM.Tipo),
                    "El tipo de medio de pago seleccionado no es válido.");
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    await CargarEmpresas(medioPagoVM.EmpresaId);
                }

                return View(medioPagoVM);
            }

            if (!medioPagoVM.EmpresaId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(medioPagoVM.EmpresaId),
                    "Debe seleccionar una empresa.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas();
                }

                return View(medioPagoVM);
            }

            int empresaId = medioPagoVM.EmpresaId.Value;

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == empresaId &&
                    e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(
                    nameof(medioPagoVM.EmpresaId),
                    "La empresa seleccionada no es válida.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(empresaId);
                }

                return View(medioPagoVM);
            }

            // Defensa adicional multiempresa
            if (!esSuperAdmin &&
                empresaId != usuario.EmpresaId)
            {
                return Forbid();
            }

            medioPagoVM.Nombre = medioPagoVM.Nombre.Trim();

            medioPagoVM.Descripcion =
                string.IsNullOrWhiteSpace(medioPagoVM.Descripcion)
                    ? null
                    : medioPagoVM.Descripcion.Trim();

            // Incluye activos e inactivos deliberadamente.
            var existente = await _context.MediosPago
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.EmpresaId == empresaId &&
                    m.Nombre.ToLower() ==
                    medioPagoVM.Nombre.ToLower());

            if (existente != null)
            {
                string mensaje = existente.Estado
                    ? "Ya existe un medio de pago con ese nombre."
                    : "Ya existe un medio de pago inactivo con ese nombre. Puede reactivarlo desde Edit.";

                ModelState.AddModelError(
                    nameof(medioPagoVM.Nombre),
                    mensaje);

                if (esSuperAdmin)
                {
                    await CargarEmpresas(empresaId);
                }

                return View(medioPagoVM);
            }

            try
            {
                var medioPago = new MedioPago
                {
                    Nombre = medioPagoVM.Nombre,
                    Descripcion = medioPagoVM.Descripcion,
                    Tipo = medioPagoVM.Tipo,
                    Estado = true,
                    FechaAlta = DateTime.Now,
                    EmpresaId = empresaId
                };

                _context.MediosPago.Add(medioPago);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Medio de pago creado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al crear el medio de pago.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(empresaId);
                }

                return View(medioPagoVM);
            }
        }
        // GET: MedioPago/Details/5
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

            IQueryable<MedioPago> consulta = _context.MediosPago
                .AsNoTracking()
                .Include(m => m.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var medioPago = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medioPago == null)
            {
                return NotFound();
            }

            var cajasAsociadas = await _context.CajaMediosPago
                .AsNoTracking()
                .Where(cm => cm.MedioPagoId == medioPago.Id)
                .Select(cm => cm.Caja.Nombre)
                .OrderBy(nombre => nombre)
                .ToListAsync();

            var medioPagoVM = new MedioPagoDetailsVM
            {
                Id = medioPago.Id,
                Nombre = medioPago.Nombre,
                Descripcion = medioPago.Descripcion,
                Tipo = medioPago.Tipo,
                Estado = medioPago.Estado,
                FechaAlta = medioPago.FechaAlta,
                EmpresaNombre = medioPago.Empresa.Nombre,
                CajasAsociadas = cajasAsociadas
            };

            return View(medioPagoVM);
        }
        // GET: MedioPago/Edit/5
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

            IQueryable<MedioPago> consulta = _context.MediosPago
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var medioPago = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medioPago == null)
            {
                return NotFound();
            }

            var medioPagoVM = new MedioPagoEditVM
            {
                Id = medioPago.Id,
                Nombre = medioPago.Nombre,
                Descripcion = medioPago.Descripcion,
                Tipo = medioPago.Tipo,
                Estado = medioPago.Estado,
                EmpresaId = medioPago.EmpresaId
            };

            return View(medioPagoVM);
        }
        // POST: MedioPago/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MedioPagoEditVM medioPagoVM)
        {
            if (id != medioPagoVM.Id)
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

            IQueryable<MedioPago> consulta = _context.MediosPago;

            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var medioPago = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medioPago == null)
            {
                return NotFound();
            }

            // La empresa real siempre viene de BD.
            medioPagoVM.EmpresaId = medioPago.EmpresaId;
            ModelState.Remove(nameof(medioPagoVM.EmpresaId));

            if (!Enum.IsDefined(medioPagoVM.Tipo))
            {
                ModelState.AddModelError(
                    nameof(medioPagoVM.Tipo),
                    "El tipo de medio de pago seleccionado no es válido.");
            }

            if (!ModelState.IsValid)
            {
                return View(medioPagoVM);
            }

            medioPagoVM.Nombre = medioPagoVM.Nombre.Trim();

            medioPagoVM.Descripcion =
                string.IsNullOrWhiteSpace(medioPagoVM.Descripcion)
                    ? null
                    : medioPagoVM.Descripcion.Trim();

            bool nombreDuplicado = await _context.MediosPago
                .AsNoTracking()
                .AnyAsync(m =>
                    m.Id != medioPago.Id &&
                    m.EmpresaId == medioPago.EmpresaId &&
                    m.Nombre.ToLower() ==
                        medioPagoVM.Nombre.ToLower());

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(medioPagoVM.Nombre),
                    "Ya existe otro medio de pago con ese nombre para esta empresa.");

                return View(medioPagoVM);
            }

            try
            {
                medioPago.Nombre = medioPagoVM.Nombre;
                medioPago.Descripcion = medioPagoVM.Descripcion;
                medioPago.Tipo = medioPagoVM.Tipo;
                medioPago.Estado = medioPagoVM.Estado;

                bool tieneMovimientos = await _context.MovimientosCaja
                    .AsNoTracking()
                    .AnyAsync(m => m.MedioPagoId == medioPago.Id);

                if (tieneMovimientos &&
                    medioPago.Tipo != medioPagoVM.Tipo)
                {
                    ModelState.AddModelError(
                        nameof(medioPagoVM.Tipo),
                        "No puede cambiar el tipo de un medio de pago que ya posee movimientos financieros.");

                    return View(medioPagoVM);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    medioPago.Estado
                        ? "Medio de pago modificado correctamente."
                        : "Medio de pago modificado y desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar el medio de pago.");

                return View(medioPagoVM);
            }
        }
        // GET: MedioPago/Delete/5
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

            IQueryable<MedioPago> consulta = _context.MediosPago
                .AsNoTracking()
                .Include(m => m.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var medioPago = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medioPago == null)
            {
                return NotFound();
            }

            if (!medioPago.Estado)
            {
                TempData["Error"] =
                    "El medio de pago ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            var cajasAsociadas = await _context.CajaMediosPago
                .AsNoTracking()
                .Where(cm => cm.MedioPagoId == medioPago.Id)
                .Select(cm => cm.Caja.Nombre)
                .OrderBy(nombre => nombre)
                .ToListAsync();

            var medioPagoVM = new MedioPagoDetailsVM
            {
                Id = medioPago.Id,
                Nombre = medioPago.Nombre,
                Descripcion = medioPago.Descripcion,
                Estado = medioPago.Estado,
                FechaAlta = medioPago.FechaAlta,
                EmpresaNombre = medioPago.Empresa.Nombre,
                CajasAsociadas = cajasAsociadas
            };

            return View(medioPagoVM);
        }
        // POST: MedioPago/Delete/5
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

            IQueryable<MedioPago> consulta = _context.MediosPago;

            if (!esSuperAdmin)
            {
                consulta = consulta
                    .Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var medioPago = await consulta
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medioPago == null)
            {
                return NotFound();
            }

            if (!medioPago.Estado)
            {
                TempData["Error"] =
                    "El medio de pago ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                medioPago.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Medio de pago desactivado correctamente.";
            }
            catch
            {
                TempData["Error"] =
                    "Ocurrió un error al desactivar el medio de pago.";
            }

            return RedirectToAction(nameof(Index));
        }

        //Helpers Methods
        private async Task CargarEmpresas(
            int? empresaId = null)
        {
            ViewData["EmpresaId"] = new SelectList(
                await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre",
                empresaId);
        }
    }
}