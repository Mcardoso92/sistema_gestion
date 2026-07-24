using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

namespace saas.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly SaasDbContext _context;

        public CategoriaController(SaasDbContext context)
        {
            _context = context;
        }

        // GET: Categoria
        public async Task<IActionResult> Index()
        {
            var saasDbContext = _context.Categorias.Include(c => c.Empresa);
            return View(await saasDbContext.ToListAsync());
        }

        // GET: Categoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .Include(c => c.Empresa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categoria/Create
        public IActionResult Create()
        {
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre");
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
                if (ModelState.IsValid)
                {
                    bool existeCategoria = await _context.Categorias.AnyAsync(c =>
                        c.EmpresaId == categoria.EmpresaId &&
                        c.Nombre.ToLower() == categoria.Nombre.ToLower());

                    if (existeCategoria)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre para esta empresa.");

                        ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
                        return View(categoria);
                    }

                    categoria.Estado = true; // Cargar el valor predeterminado de Estado como true al crear una nueva categoría
                    _context.Add(categoria);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Categoria creada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al Crear la Categoria.");

                return View(categoria);
            }            

            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
            return View(categoria);
        }

        // GET: Categoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
            return View(categoria);
        }

        // POST: Categoria/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            var categoriaDb = await _context.Categorias.FindAsync(id);
            if (categoriaDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool existeCategoria = await _context.Categorias.AnyAsync(c =>
                        c.EmpresaId == categoria.EmpresaId &&
                        c.Nombre.ToLower() == categoria.Nombre.ToLower() &&
                        c.Id != categoria.Id);

                    if (existeCategoria)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre para esta empresa.");

                        ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);

                        return View(categoria);
                    }

                    categoriaDb.Nombre = categoria.Nombre;
                    categoriaDb.Estado = categoria.Estado;
                    categoriaDb.EmpresaId = categoria.EmpresaId;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Categoria modificada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", categoria.EmpresaId);
            return View(categoria);
        }

        // GET: Categoria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .Include(c => c.Empresa)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria != null)
                {
                    _context.Categorias.Remove(categoria);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoria eliminada correctamente.";
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
