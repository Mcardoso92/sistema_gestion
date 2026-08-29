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
        public async Task<IActionResult> Historial(StockHistorialVM historialVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (historialVM.ProductoId.HasValue)
            {
                IQueryable<Producto> productoConsulta = _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Categoria)
                    .Include(p => p.Empresa);

                if (!esSuperAdmin)
                {
                    productoConsulta = productoConsulta.Where(p => p.EmpresaId == usuario.EmpresaId);
                }
                else if (historialVM.EmpresaId.HasValue)
                {
                    productoConsulta = productoConsulta.Where(p => p.EmpresaId == historialVM.EmpresaId.Value);
                }

                var producto = await productoConsulta
                    .FirstOrDefaultAsync(p => p.Id == historialVM.ProductoId.Value);

                if (producto == null)
                {
                    return NotFound();
                }

                historialVM.ProductoNombre = producto.Nombre;
                historialVM.CodigoBarra = producto.CodigoBarra;
                historialVM.CategoriaNombre = producto.Categoria.Nombre;
                historialVM.EmpresaNombre = producto.Empresa.Nombre;
                historialVM.StockActual = producto.Stock;
                historialVM.PuntoReposicion = producto.PuntoReposicion;
                historialVM.ProductoActivo = producto.Estado;
            }

            IQueryable<MovimientoStock> consulta = _context.MovimientosStock
                .AsNoTracking()
                .Include(m => m.Producto)
                .Include(m => m.Empresa)
                .Include(m => m.Usuario);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(m => m.EmpresaId == usuario.EmpresaId);
            }
            else if (historialVM.EmpresaId.HasValue)
            {
                consulta = consulta.Where(m => m.EmpresaId == historialVM.EmpresaId.Value);
            }

            if (historialVM.ProductoId.HasValue)
            {
                consulta = consulta.Where(m => m.ProductoId == historialVM.ProductoId.Value);
            }

            if (historialVM.Tipo.HasValue)
            {
                consulta = consulta.Where(m => m.Tipo == historialVM.Tipo.Value);
            }

            if (historialVM.FechaDesde.HasValue)
            {
                DateTime fechaDesde = historialVM.FechaDesde.Value.Date;
                consulta = consulta.Where(m => m.Fecha >= fechaDesde);
            }

            if (historialVM.FechaHasta.HasValue)
            {
                DateTime fechaHasta = historialVM.FechaHasta.Value.Date.AddDays(1);
                consulta = consulta.Where(m => m.Fecha < fechaHasta);
            }

            historialVM.Movimientos = await consulta
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.Id)
                .Select(m => new StockHistorialItemVM
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    ProductoNombre = m.Producto.Nombre,
                    CodigoBarra = m.Producto.CodigoBarra,
                    EmpresaNombre = m.Empresa.Nombre,
                    UsuarioNombre = m.Usuario.Nombre + " " + m.Usuario.Apellido,
                    Tipo = m.Tipo,
                    Cantidad = m.Cantidad,
                    StockAnterior = m.StockAnterior,
                    StockPosterior = m.StockPosterior,
                    Motivo = m.Motivo,
                    VentaId = m.VentaId
                })
                .ToListAsync();

            IQueryable<Producto> productosConsulta = _context.Productos
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                productosConsulta = productosConsulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }
            else if (historialVM.EmpresaId.HasValue)
            {
                productosConsulta = productosConsulta.Where(p => p.EmpresaId == historialVM.EmpresaId.Value);
            }

            historialVM.Productos = await productosConsulta
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                historialVM.Empresas = await _context.Empresas
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

            return View(historialVM);
        }
        // GET: MovimientoStock/Details/5
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

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<MovimientoStock> consulta = _context.MovimientosStock
                .AsNoTracking()
                .Include(m => m.Empresa)
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .Include(m => m.Venta);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(m => m.EmpresaId == usuario.EmpresaId);
            }

            var movimientoStock = await consulta.FirstOrDefaultAsync(m => m.Id == id);

            if (movimientoStock == null)
            {
                return NotFound();
            }

            return View(movimientoStock);
        }
    }
}
