using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;
using saas.ViewModel.Enums;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class MovimientoStockController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public MovimientoStockController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MovimientoStock
        public async Task<IActionResult> Index(StockIndexVM stockVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            IQueryable<Producto> consulta = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p =>
                    p.EmpresaId == usuario.EmpresaId);
            }
            else if (stockVM.EmpresaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.EmpresaId == stockVM.EmpresaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(stockVM.Busqueda))
            {
                string busqueda = stockVM.Busqueda.Trim();

                consulta = consulta.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    (
                        p.CodigoBarra != null &&
                        p.CodigoBarra.Contains(busqueda)
                    ));
            }

            switch (stockVM.EstadoStock?.ToLower())
            {
                case "constock":
                    consulta = consulta.Where(p =>
                        p.Stock > p.PuntoReposicion);
                    break;

                case "bajo":
                    consulta = consulta.Where(p =>
                        p.Stock > 0 &&
                        p.Stock <= p.PuntoReposicion);
                    break;

                case "sinstock":
                    consulta = consulta.Where(p =>
                        p.Stock == 0);
                    break;

                default:
                    stockVM.EstadoStock = "todos";
                    break;
            }

            stockVM.Productos = await consulta
                .OrderBy(p => p.Nombre)
                .Select(p => new StockIndexItemVM
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarra = p.CodigoBarra,
                    CategoriaNombre = p.Categoria.Nombre,
                    EmpresaNombre = p.Empresa.Nombre,
                    Stock = p.Stock,
                    PuntoReposicion = p.PuntoReposicion,
                    ProductoActivo = p.Estado,

                    EstadoStock =
                        p.Stock == 0
                            ? EstadoStockVM.SinStock
                            : p.Stock <= p.PuntoReposicion
                                ? EstadoStockVM.StockBajo
                                : EstadoStockVM.ConStock
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                stockVM.Empresas = await _context.Empresas
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

            return View(stockVM);
        }

        [HttpGet]
        [Authorize(Roles = "AdminEmpresa")]
        public async Task<IActionResult> Ajustar(int productoId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == productoId &&
                    p.EmpresaId == usuario.EmpresaId);

            if (producto == null)
            {
                return NotFound();
            }

            if (!producto.Estado)
            {
                TempData["Error"] = "No se puede ajustar el stock de un producto inactivo.";
                return RedirectToAction(nameof(Index));
            }

            var ajusteVM = new StockAjusteVM
            {
                ProductoId = producto.Id,
                ProductoNombre = producto.Nombre,
                CodigoBarra = producto.CodigoBarra,
                StockActual = producto.Stock
            };

            return View(ajusteVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "AdminEmpresa")]
        public async Task<IActionResult> Ajustar(StockAjusteVM ajusteVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.Id == ajusteVM.ProductoId &&
                    p.EmpresaId == usuario.EmpresaId);

            if (producto == null)
            {
                return NotFound();
            }

            ajusteVM.ProductoNombre = producto.Nombre;
            ajusteVM.CodigoBarra = producto.CodigoBarra;
            ajusteVM.StockActual = producto.Stock;

            if (!producto.Estado)
            {
                ModelState.AddModelError("", "No se puede ajustar el stock de un producto inactivo.");
                return View(ajusteVM);
            }

            if (!ModelState.IsValid)
            {
                return View(ajusteVM);
            }

            int stockAnterior = producto.Stock;
            int stockPosterior;
            TipoMovimientoStock tipoMovimiento;

            switch (ajusteVM.Tipo)
            {
                case TipoAjusteStockVM.Entrada:
                    stockPosterior = stockAnterior + ajusteVM.Cantidad;
                    tipoMovimiento = TipoMovimientoStock.AjusteEntrada;
                    break;

                case TipoAjusteStockVM.Salida:
                    if (ajusteVM.Cantidad > stockAnterior)
                    {
                        ModelState.AddModelError(
                            "Cantidad",
                            "La cantidad a retirar no puede superar el stock disponible.");

                        return View(ajusteVM);
                    }

                    stockPosterior = stockAnterior - ajusteVM.Cantidad;
                    tipoMovimiento = TipoMovimientoStock.AjusteSalida;
                    break;

                default:
                    ModelState.AddModelError(
                        "Tipo",
                        "El tipo de ajuste seleccionado no es válido.");

                    return View(ajusteVM);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                DateTime fecha = DateTime.Now;

                producto.Stock = stockPosterior;

                var movimiento = new MovimientoStock
                {
                    ProductoId = producto.Id,
                    EmpresaId = producto.EmpresaId,
                    Tipo = tipoMovimiento,
                    Cantidad = ajusteVM.Cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = stockPosterior,
                    Motivo = ajusteVM.Motivo.Trim(),
                    Fecha = fecha,
                    UsuarioId = usuario.Id
                };

                _context.MovimientosStock.Add(movimiento);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Stock ajustado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                producto.Stock = stockAnterior;
                ajusteVM.StockActual = stockAnterior;

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al realizar el ajuste de stock.");

                return View(ajusteVM);
            }
        }

        // GET: MovimientoStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoStock = await _context.MovimientosStock
                .Include(m => m.Empresa)
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .Include(m => m.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoStock == null)
            {
                return NotFound();
            }

            return View(movimientoStock);
        }

        // GET: MovimientoStock/Create
        public IActionResult Create()
        {
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre");
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre");
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id");
            return View();
        }

        // POST: MovimientoStock/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductoId,EmpresaId,Tipo,Cantidad,StockAnterior,StockPosterior,Motivo,Fecha,UsuarioId,VentaId")] MovimientoStock movimientoStock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movimientoStock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", movimientoStock.EmpresaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", movimientoStock.ProductoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id", movimientoStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", movimientoStock.VentaId);
            return View(movimientoStock);
        }

        // GET: MovimientoStock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoStock = await _context.MovimientosStock.FindAsync(id);
            if (movimientoStock == null)
            {
                return NotFound();
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", movimientoStock.EmpresaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", movimientoStock.ProductoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id", movimientoStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", movimientoStock.VentaId);
            return View(movimientoStock);
        }

        // POST: MovimientoStock/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductoId,EmpresaId,Tipo,Cantidad,StockAnterior,StockPosterior,Motivo,Fecha,UsuarioId,VentaId")] MovimientoStock movimientoStock)
        {
            if (id != movimientoStock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimientoStock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoStockExists(movimientoStock.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpresaId"] = new SelectList(_context.Empresas, "Id", "Nombre", movimientoStock.EmpresaId);
            ViewData["ProductoId"] = new SelectList(_context.Productos, "Id", "Nombre", movimientoStock.ProductoId);
            ViewData["UsuarioId"] = new SelectList(_context.Users, "Id", "Id", movimientoStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Ventas, "Id", "Id", movimientoStock.VentaId);
            return View(movimientoStock);
        }

        // GET: MovimientoStock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimientoStock = await _context.MovimientosStock
                .Include(m => m.Empresa)
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .Include(m => m.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimientoStock == null)
            {
                return NotFound();
            }

            return View(movimientoStock);
        }

        // POST: MovimientoStock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movimientoStock = await _context.MovimientosStock.FindAsync(id);
            if (movimientoStock != null)
            {
                _context.MovimientosStock.Remove(movimientoStock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovimientoStockExists(int id)
        {
            return _context.MovimientosStock.Any(e => e.Id == id);
        }
    }
}
