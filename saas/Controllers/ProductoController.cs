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
    public class ProductoController : Controller
    {
        private readonly SaasDbContext _context;

        public ProductoController(SaasDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public async Task<IActionResult> Index()
        {
            var saasDbContext = _context.Productos.Include(p => p.Categoria).Include(p => p.Empresa);
            return View(await saasDbContext.ToListAsync());
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Producto/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(
                _context.Categorias.Where(c => c.Estado),
                "Id",
                "Nombre");
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre");
            var producto = new Producto
            {
                Estado = true
            };
            return View();
        }

        // POST: Producto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool existe = await _context.Productos.AnyAsync(p =>
                        p.EmpresaId == producto.EmpresaId &&
                        p.Nombre.ToLower() == producto.Nombre.ToLower());

                    if (existe)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre para esta empresa.");

                    }

                    _context.Add(producto);
                    producto.FechaAlta = DateTime.Now;
                    producto.Estado = true;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {

                ModelState.AddModelError("", "Ocurrió un error al Crear el Producto.");

                return View(producto);
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", producto.EmpresaId);

            return View(producto);
        }

        // GET: Producto/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", producto.EmpresaId);
            return View(producto);
        }

        // POST: Producto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            var productoDb = await _context.Productos.FindAsync(id);

            if (productoDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool existe = await _context.Productos.AnyAsync(p =>
                        p.EmpresaId == producto.EmpresaId &&
                        p.Nombre.ToLower() == producto.Nombre.ToLower() &&
                        p.Id != producto.Id); // Excluir el producto actual de la verificación de existencia

                    if (existe)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre para esta empresa.");

                    }
                    productoDb.Nombre = producto.Nombre;
                    productoDb.CodigoBarra = producto.CodigoBarra;
                    productoDb.Descripcion = producto.Descripcion;
                    productoDb.CategoriaId = producto.CategoriaId;
                    productoDb.PrecioCosto = producto.PrecioCosto;
                    productoDb.PrecioVenta = producto.PrecioVenta;
                    productoDb.Stock = producto.Stock;
                    productoDb.PuntoReposicion = producto.PuntoReposicion;
                    productoDb.Estado = producto.Estado;
                    productoDb.UrlImagen = producto.UrlImagen;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Producto modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", producto.EmpresaId);

            return View(producto);
        }

        // GET: Producto/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Empresa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
