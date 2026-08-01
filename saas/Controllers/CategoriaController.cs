using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class CategoriaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public CategoriaController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Categoria
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Categoria> categorias = _context.Categorias
                .Where(c => c.Estado)
                .Include(c => c.Empresa);

            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                categorias = categorias.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            return View(await categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync());
        }

        // GET: Categoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Categoria> consulta = _context.Categorias
                .Include(c => c.Empresa);

            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categoria/Create
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            if (await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre");
            }

            return View();
        }

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var usuarioModel = await _userManager.GetUserAsync(User);

                    if (usuarioModel != null && await _userManager.IsInRoleAsync(usuarioModel, "SuperAdmin"))
                    {
                        ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
                    }

                    return View(categoria);
                }
                var usuario = await _userManager.GetUserAsync(User);

                if (usuario == null)
                {
                    return Challenge();
                }

                bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

                if (!esSuperAdmin)
                {
                    categoria.EmpresaId = usuario.EmpresaId;
                }

                bool existeCategoria = await _context.Categorias.AnyAsync(c =>
                        c.EmpresaId == categoria.EmpresaId &&
                        c.Nombre.ToLower() == categoria.Nombre.ToLower());

                if (existeCategoria)
                {
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre para esta empresa.");
                    if (esSuperAdmin)
                    {
                        ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
                    }

                    return View(categoria);
                }

                categoria.Estado = true;
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {

                var usuario = await _userManager.GetUserAsync(User);

                if (usuario != null &&
                    await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
                {
                    ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
                }

                ModelState.AddModelError("", "Ocurrió un error al crear la categoría.");

                return View(categoria);
            }
        }

        // GET: Categoria/Edit/5
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

            IQueryable<Categoria> consulta = _context.Categorias;

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (esSuperAdmin)
            {
                ViewData["EmpresaId"] = new SelectList(
                    _context.Empresas,
                    "Id",
                    "Nombre",
                    categoria.EmpresaId);
            }

            return View(categoria);
        }

        // POST: Categoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                categoria.EmpresaId = usuario.EmpresaId;
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    ViewData["EmpresaId"] = new SelectList(
                        _context.Empresas,
                        "Id",
                        "Nombre",
                        categoria.EmpresaId);
                }

                return View(categoria);
            }

            bool existeCategoria = await _context.Categorias.AnyAsync(c =>
                c.Id != categoria.Id &&
                c.EmpresaId == categoria.EmpresaId &&
                c.Nombre.ToLower() == categoria.Nombre.ToLower());

            if (existeCategoria)
            {
                ModelState.AddModelError("Nombre",
                    "Ya existe una categoría con ese nombre para esta empresa.");

                if (esSuperAdmin)
                {
                    ViewData["EmpresaId"] = new SelectList(
                        _context.Empresas,
                        "Id",
                        "Nombre",
                        categoria.EmpresaId);
                }

                return View(categoria);
            }

            try
            {
                _context.Update(categoria);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Categoría modificada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al modificar la categoría.");

                if (esSuperAdmin)
                {
                    ViewData["EmpresaId"] = new SelectList(
                        _context.Empresas,
                        "Id",
                        "Nombre",
                        categoria.EmpresaId);
                }

                return View(categoria);
            }
        }

        // GET: Categoria/Delete/5
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

            IQueryable<Categoria> consulta = _context.Categorias
                .Include(c => c.Empresa);

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categoria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Categoria> consulta = _context.Categorias;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var categoria = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            try
            {
                categoria.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Categoría desactivada correctamente.";
            }
            catch
            {

                TempData["Error"] = "No es posible desactivar la categoría porque tiene información relacionada.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}