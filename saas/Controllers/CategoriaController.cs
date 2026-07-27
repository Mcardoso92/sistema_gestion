using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Include(c => c.Empresa);

            // Si es SuperAdmin ve todas las categorías
            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                // Si no es SuperAdmin, solo ve las de su empresa
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                // Si NO es SuperAdmin, la empresa siempre es la del usuario
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

                categoria.Estado = true; // Cargar el valor predeterminado de Estado como true al crear una nueva categoría
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoria creada correctamente.";
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            // Si no es SuperAdmin, siempre pertenece a su empresa
            if (!esSuperAdmin)
            {
                categoria.EmpresaId = usuario.EmpresaId;
            }

            // Validación
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

            // Verificar nombre duplicado
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
                _context.Categorias.Remove(categoria);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Categoría eliminada correctamente.";
            }
            catch
            {

                TempData["Error"] = "No es posible eliminar la categoria porque tiene información relacionada.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
