using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

namespace saas.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ProductoController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Producto
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Producto> productos = _context.Productos
                .Where(p => p.Estado)
                .Include(p => p.Empresa);

            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                productos = productos.Where(p =>
                p.EmpresaId == usuario.EmpresaId);
            }

            return View(await productos
                .Include(c => c.Categoria)
                .OrderBy(c => c.Nombre)
                .ToListAsync());
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Producto> consulta = _context.Productos
                .Include(p => p.Empresa)
                .Include(p => p.Categoria);

            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var producto = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Producto/Create
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (esSuperAdmin)
            {
                ViewData["EmpresaId"] = new SelectList(_context.Empresas.Where(e => e.Estado), "Id", "Nombre");
                ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Estado), "Id", "Nombre");
            }
            else
            {
                ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => c.Estado && c.EmpresaId == usuario.EmpresaId), "Id", "Nombre");
            }

            var producto = new Producto
            {
                Estado = true
            };

            return View(producto);
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            try
            {
                if (!esSuperAdmin)
                {
                    producto.EmpresaId = usuario.EmpresaId;
                }

                if (!ModelState.IsValid)
                {
                    CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                    return View(producto);
                }

                // Verificar que la categoría pertenezca a la empresa
                bool categoriaValida = await _context.Categorias.AnyAsync(c =>
                    c.Id == producto.CategoriaId &&
                    c.EmpresaId == producto.EmpresaId &&
                    c.Estado);

                if (!categoriaValida)
                {
                    ModelState.AddModelError("CategoriaId", "La categoría seleccionada no es válida.");

                    CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                    return View(producto);
                }

                bool existeProducto = await _context.Productos.AnyAsync(p =>
                    p.EmpresaId == producto.EmpresaId &&
                    p.Nombre.ToLower() == producto.Nombre.ToLower());

                if (existeProducto)
                {
                    ModelState.AddModelError("Nombre",
                        "Ya existe un producto con ese nombre para esta empresa.");

                    CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                    return View(producto);
                }

                producto.FechaAlta = DateTime.Now;
                producto.Estado = true;
                _context.Productos.Add(producto);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {

                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                ModelState.AddModelError("", "Ocurrió un error al crear el producto.");

                return View(producto);
            }
        }

        // GET: Producto/Edit/5
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

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Producto> consulta = _context.Productos;


            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var producto = await consulta.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

            return View(producto);
        }

        // POST: Producto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id)
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
                producto.EmpresaId = usuario.EmpresaId;
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                return View(producto);
            }

            bool categoriaValida = await _context.Categorias.AnyAsync(c =>
                c.Id == producto.CategoriaId &&
                c.EmpresaId == producto.EmpresaId &&
                c.Estado);

            if (!categoriaValida)
            {
                ModelState.AddModelError("CategoriaId", "La categoría seleccionada no es válida.");

                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                return View(producto);
            }

            bool existeProducto = await _context.Productos.AnyAsync(c =>
                c.Id != producto.Id &&
                c.EmpresaId == producto.EmpresaId &&
                c.Nombre.ToLower() == producto.Nombre.ToLower());

            if (existeProducto)
            {
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre para esta empresa.");
                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);
                return View(producto);
            }

            var productoDB = await _context.Productos.FindAsync(id);

            if (productoDB == null)
            {
                return NotFound();
            }

            try
            {

                productoDB.Nombre = producto.Nombre;
                productoDB.CodigoBarra = producto.CodigoBarra;
                productoDB.Descripcion = producto.Descripcion;
                productoDB.CategoriaId = producto.CategoriaId;
                productoDB.PrecioCosto = producto.PrecioCosto;
                productoDB.PrecioVenta = producto.PrecioVenta;
                productoDB.Stock = producto.Stock;
                productoDB.PuntoReposicion = producto.PuntoReposicion;
                productoDB.Estado = producto.Estado;
                productoDB.UrlImagen = producto.UrlImagen;
                productoDB.EmpresaId = producto.EmpresaId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                ModelState.AddModelError("", "Ocurrió un error al modificar el producto.");

                return View(producto);
            }
        }

        // GET: Producto/Delete/5
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

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Producto> consulta = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var producto = await consulta.FirstOrDefaultAsync(p => p.Id == id);

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
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Producto> consulta = _context.Productos;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var producto = await consulta.FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            try
            {
                producto.Estado = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Producto desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "No fue posible desactivar el producto porque tiene información relacionada.";

                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private void CargarCombos(int empresaId, int categoriaId, Usuario usuario, bool esSuperAdmin)
        {
            if (esSuperAdmin)
            {
                ViewData["EmpresaId"] = new SelectList(
                    _context.Empresas.Where(e => e.Estado),
                    "Id",
                    "Nombre",
                    empresaId);

                ViewData["CategoriaId"] = new SelectList(
                    _context.Categorias.Where(c => c.Estado),
                    "Id",
                    "Nombre",
                    categoriaId);
            }
            else
            {
                ViewData["CategoriaId"] = new SelectList(
                    _context.Categorias
                        .Where(c => c.EmpresaId == usuario.EmpresaId && c.Estado),
                    "Id",
                    "Nombre",
                    categoriaId);
            }
        }
    }
}