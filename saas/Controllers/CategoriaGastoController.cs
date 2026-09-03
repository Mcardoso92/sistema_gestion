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
    public class CategoriaGastoController : VeltikaController
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public CategoriaGastoController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CategoriaGasto
        public async Task<IActionResult> Index(string estado = "activos", int? empresaId = null, string? busqueda = null, int pagina = 1)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto
                    .AsNoTracking()
                    .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);

                empresaId = null;
            }
            else if (empresaId.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == empresaId.Value);
            }

            switch (estado.ToLower())
            {
                case "inactivos":
                    consulta = consulta.Where(c => !c.Estado);
                    break;

                case "todos":
                    break;

                default:
                    consulta = consulta.Where(c => c.Estado);
                    estado = "activos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                consulta = consulta.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    (c.Descripcion != null &&
                     c.Descripcion.Contains(busqueda)));
            }

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalCategorias = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalCategorias / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalCategorias;

            var categorias = await consulta
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(c => new CategoriaGastoIndexVM
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Estado = c.Estado,
                    FechaAlta = c.FechaAlta,
                    EmpresaNombre = c.Empresa.Nombre
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

            return View(categorias);
        }
        // GET: CategoriaGasto/Create
        [HttpGet]
        public async Task<IActionResult> Create()
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

            var categoriaVM =
                new CategoriaGastoCreateVM();

            if (esSuperAdmin)
            {
                await CargarEmpresas();
            }
            else
            {
                categoriaVM.EmpresaId =
                    usuario.EmpresaId;
            }

            return View(categoriaVM);
        }
        // POST: CategoriaGasto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaGastoCreateVM categoriaVM)
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

            if (!esSuperAdmin)
            {
                categoriaVM.EmpresaId =
                    usuario.EmpresaId;

                ModelState.Remove(
                    nameof(categoriaVM.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    await CargarEmpresas(
                        categoriaVM.EmpresaId);
                }

                return View(categoriaVM);
            }

            if (!categoriaVM.EmpresaId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(categoriaVM.EmpresaId),
                    "Debe seleccionar una empresa.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas();
                }

                return View(categoriaVM);
            }

            int empresaId =
                categoriaVM.EmpresaId.Value;

            bool empresaValida =
                await _context.Empresas
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.Id == empresaId &&
                        e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(
                    nameof(categoriaVM.EmpresaId),
                    "La empresa seleccionada no es válida.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(
                        empresaId);
                }

                return View(categoriaVM);
            }

            if (!esSuperAdmin &&
                empresaId != usuario.EmpresaId)
            {
                return Forbid();
            }

            categoriaVM.Nombre =
                categoriaVM.Nombre.Trim();

            categoriaVM.Descripcion =
                string.IsNullOrWhiteSpace(
                    categoriaVM.Descripcion)
                    ? null
                    : categoriaVM.Descripcion.Trim();

            var existente =
                await _context.CategoriasGasto
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.EmpresaId == empresaId &&
                        c.Nombre.ToLower() ==
                        categoriaVM.Nombre.ToLower());

            if (existente != null)
            {
                string mensaje =
                    existente.Estado
                        ? "Ya existe una categoría de gasto con ese nombre."
                        : "Ya existe una categoría de gasto inactiva con ese nombre. Puede reactivarla desde la edición.";

                ModelState.AddModelError(
                    nameof(categoriaVM.Nombre),
                    mensaje);

                if (esSuperAdmin)
                {
                    await CargarEmpresas(
                        empresaId);
                }

                return View(categoriaVM);
            }

            try
            {
                var categoria =
                    new CategoriaGasto
                    {
                        Nombre =
                            categoriaVM.Nombre,
                        Descripcion =
                            categoriaVM.Descripcion,
                        Estado = true,
                        FechaAlta = DateTime.Now,
                        EmpresaId = empresaId
                    };

                _context.CategoriasGasto.Add(
                    categoria);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Categoría de gasto creada correctamente.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al crear la categoría de gasto.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(
                        empresaId);
                }

                return View(categoriaVM);
            }
        }
        // GET: CategoriaGasto/Details/5
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

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto
                    .AsNoTracking()
                    .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            var categoriaVM = new CategoriaGastoDetailsVM
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Estado = categoria.Estado,
                FechaAlta = categoria.FechaAlta,
                EmpresaNombre = categoria.Empresa.Nombre
            };

            return View(categoriaVM);
        }
        // GET: CategoriaGasto/Edit/5
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

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            var categoriaVM = new CategoriaGastoEditVM
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Estado = categoria.Estado,
                EmpresaId = categoria.EmpresaId
            };

            return View(categoriaVM);
        }
        // POST: CategoriaGasto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CategoriaGastoEditVM categoriaVM)
        {
            if (id != categoriaVM.Id)
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

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            categoriaVM.EmpresaId = categoria.EmpresaId;
            ModelState.Remove(nameof(categoriaVM.EmpresaId));

            if (!ModelState.IsValid)
            {
                return View(categoriaVM);
            }

            categoriaVM.Nombre =
                categoriaVM.Nombre.Trim();

            categoriaVM.Descripcion =
                string.IsNullOrWhiteSpace(categoriaVM.Descripcion)
                    ? null
                    : categoriaVM.Descripcion.Trim();

            bool nombreDuplicado =
                await _context.CategoriasGasto
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.Id != categoria.Id &&
                        c.EmpresaId == categoria.EmpresaId &&
                        c.Nombre.ToLower() ==
                            categoriaVM.Nombre.ToLower());

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(categoriaVM.Nombre),
                    "Ya existe otra categoría de gasto con ese nombre para esta empresa.");

                return View(categoriaVM);
            }

            try
            {
                categoria.Nombre = categoriaVM.Nombre;
                categoria.Descripcion = categoriaVM.Descripcion;
                categoria.Estado = categoriaVM.Estado;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    categoria.Estado
                        ? "Categoría de gasto modificada correctamente."
                        : "Categoría de gasto modificada y desactivada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar la categoría de gasto.");

                return View(categoriaVM);
            }
        }
        // GET: CategoriaGasto/Delete/5
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

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto
                    .AsNoTracking()
                    .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (!categoria.Estado)
            {
                TempData["Error"] =
                    "La categoría de gasto ya se encuentra inactiva.";

                return RedirectToAction(nameof(Index));
            }

            var categoriaVM = new CategoriaGastoDetailsVM
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Estado = categoria.Estado,
                FechaAlta = categoria.FechaAlta,
                EmpresaNombre = categoria.Empresa.Nombre
            };

            return View(categoriaVM);
        }
        // POST: CategoriaGasto/Delete/5
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

            IQueryable<CategoriaGasto> consulta =
                _context.CategoriasGasto;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (!categoria.Estado)
            {
                TempData["Error"] =
                    "La categoría de gasto ya se encuentra inactiva.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                categoria.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Categoría de gasto desactivada correctamente.";
            }
            catch
            {
                TempData["Error"] =
                    "Ocurrió un error al desactivar la categoría de gasto.";
            }

            return RedirectToAction(nameof(Index));
        }



        //Helpers Methods
        private async Task CargarEmpresas(
            int? empresaId = null)
        {
            ViewData["EmpresaId"] =
                new SelectList(
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
