using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ProductoController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IImagenService _imagenService;

        public ProductoController(SaasDbContext context, UserManager<Usuario> userManager, IImagenService imagenService)
        {
            _context = context;
            _userManager = userManager;
            _imagenService = imagenService;
        }

        // GET: Producto
        public async Task<IActionResult> Index(string estado = "activos", int? categoriaId = null, int? empresaId = null, string? busqueda = null, int pagina = 1)
        {
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            IQueryable<Producto> productos = _context.Productos
                .AsNoTracking()
                .Include(p => p.Empresa)
                .Include(p => p.Categoria);

            if (!esSuperAdmin)
            {
                empresaId = usuarioLogueado.EmpresaId;
                productos = productos.Where(p => p.EmpresaId == usuarioLogueado.EmpresaId);
            }
            else if (empresaId.HasValue)
            {
                productos = productos.Where(p => p.EmpresaId == empresaId.Value);
            }

            switch (estado.ToLower())
            {
                case "inactivos":
                    productos = productos.Where(p => !p.Estado);
                    break;

                case "todos":
                    break;

                default:
                    productos = productos.Where(p => p.Estado);
                    estado = "activos";
                    break;
            }

            if (categoriaId.HasValue)
            {
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                productos = productos.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    (p.CodigoBarra != null && p.CodigoBarra.Contains(busqueda)));
            }

            if (esSuperAdmin)
            {
                ViewBag.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
            }

            IQueryable<Categoria> categorias = _context.Categorias
                .AsNoTracking()
                .Where(c => c.Estado);

            if (empresaId.HasValue)
            {
                categorias = categorias.Where(c => c.EmpresaId == empresaId.Value);
            }

            ViewBag.Categorias = await categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Estado = estado;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.EmpresaId = esSuperAdmin ? empresaId : null;
            ViewBag.Busqueda = busqueda;

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);

            int totalProductos = await productos.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalProductos / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProductos = totalProductos;

            var listaProductos = await productos
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            return View(listaProductos);
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
        public async Task<IActionResult> Create([Bind("CodigoBarra,Nombre,Descripcion,CategoriaId,PrecioCosto,PrecioVenta,Stock,PuntoReposicion,EmpresaId")] Producto producto, IFormFile? imagenArchivo)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            string? rutaImagenNueva = null;

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

                bool empresaValida = await _context.Empresas.AnyAsync(e =>
                    e.Id == producto.EmpresaId &&
                    e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(producto.EmpresaId),
                        "La empresa seleccionada no es válida o se encuentra inactiva.");

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

                DateTime fecha = DateTime.Now;

                producto.FechaAlta = fecha;
                producto.Estado = true;

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Productos.Add(producto);

                await _context.SaveChangesAsync();

                if (imagenArchivo != null)
                {
                    ResultadoImagen resultadoImagen = await _imagenService.GuardarAsync(imagenArchivo, producto.EmpresaId, "productos", producto.Id.ToString());

                    if (!resultadoImagen.Exito)
                    {
                        ModelState.AddModelError("imagenArchivo", resultadoImagen.Error!);
                        await transaction.RollbackAsync();
                        CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                        return View(producto);
                    }

                    rutaImagenNueva = resultadoImagen.Ruta;
                    producto.UrlImagen = rutaImagenNueva;

                    await _context.SaveChangesAsync();
                }

                if (producto.Stock > 0)
                {
                    var movimientoStock = new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        EmpresaId = producto.EmpresaId,
                        Tipo = TipoMovimientoStock.StockInicial,
                        Cantidad = producto.Stock,
                        StockAnterior = 0,
                        StockPosterior = producto.Stock,
                        Motivo = "Stock inicial",
                        Fecha = fecha,
                        UsuarioId = usuario.Id
                    };

                    _context.MovimientosStock.Add(movimientoStock);

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Success"] = "Producto creado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _imagenService.Eliminar(rutaImagenNueva);

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodigoBarra,Nombre,Descripcion,CategoriaId,PrecioCosto,PrecioVenta,PuntoReposicion,Estado,EmpresaId")] Producto producto, IFormFile? imagenArchivo, bool eliminarImagen = false)
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

            IQueryable<Producto> consulta = _context.Productos;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var productoDb = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (productoDb == null)
            {
                return NotFound();
            }

            if (!esSuperAdmin)
            {
                producto.EmpresaId = usuario.EmpresaId;
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                return View(producto);
            }

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == producto.EmpresaId &&
                e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(
                    nameof(producto.EmpresaId),
                    "La empresa seleccionada no es válida o se encuentra inactiva.");

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

            string? rutaAnterior = productoDb.UrlImagen;
            string? rutaNueva = null;

            try
            {
                if (imagenArchivo != null)
                {
                    ResultadoImagen resultadoImagen = await _imagenService.GuardarAsync(imagenArchivo, producto.EmpresaId, "productos", producto.Id.ToString());

                    if (!resultadoImagen.Exito)
                    {
                        ModelState.AddModelError("imagenArchivo", resultadoImagen.Error!);
                        producto.UrlImagen = rutaAnterior;
                        CargarCombos(producto.EmpresaId, producto.CategoriaId, usuario, esSuperAdmin);

                        return View(producto);
                    }

                    rutaNueva = resultadoImagen.Ruta;
                    productoDb.UrlImagen = rutaNueva;
                }
                else if (eliminarImagen)
                {
                    productoDb.UrlImagen = null;
                }

                productoDb.Nombre = producto.Nombre;
                productoDb.CodigoBarra = producto.CodigoBarra;
                productoDb.Descripcion = producto.Descripcion;
                productoDb.CategoriaId = producto.CategoriaId;
                productoDb.PrecioCosto = producto.PrecioCosto;
                productoDb.PrecioVenta = producto.PrecioVenta;
                productoDb.PuntoReposicion = producto.PuntoReposicion;
                productoDb.Estado = producto.Estado;
                productoDb.EmpresaId = producto.EmpresaId;

                await _context.SaveChangesAsync();

                if (rutaNueva != null || eliminarImagen)
                {
                    _imagenService.Eliminar(rutaAnterior);
                }

                TempData["Success"] = "Producto modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _imagenService.Eliminar(rutaNueva);
                producto.UrlImagen = rutaAnterior;

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
                TempData["Error"] = "Ocurrió un error al desactivar el producto.";

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