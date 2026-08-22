using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class CompraController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly CompraSaldoService _compraSaldoService;

        public CompraController(SaasDbContext context, UserManager<Usuario> userManager, CompraSaldoService compraSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _compraSaldoService = compraSaldoService;
        }

        // GET: Compra
        public async Task<IActionResult> Index(CompraIndexVM compraVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Compra> consulta = _context.Compras
                .AsNoTracking()
                .Include(c => c.Empresa)
                .Include(c => c.Proveedor);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
                compraVM.EmpresaId = null;
            }
            else if (compraVM.EmpresaId.HasValue)
            {
                consulta = consulta.Where(c => c.EmpresaId == compraVM.EmpresaId.Value);
            }

            switch (compraVM.Estado?.ToLower())
            {
                case "anuladas":
                    consulta = consulta.Where(c => !c.Estado);
                    break;

                case "todas":
                    break;

                default:
                    consulta = consulta.Where(c => c.Estado);
                    compraVM.Estado = "activas";
                    break;
            }

            if (compraVM.ProveedorId.HasValue)
            {
                consulta = consulta.Where(c => c.ProveedorId == compraVM.ProveedorId.Value);
            }

            if (compraVM.FechaDesde.HasValue)
            {
                DateTime fechaDesde = compraVM.FechaDesde.Value.Date;

                consulta = consulta.Where(c => c.Fecha >= fechaDesde);
            }

            if (compraVM.FechaHasta.HasValue)
            {
                DateTime fechaHasta = compraVM.FechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(c => c.Fecha < fechaHasta);
            }

            if (!string.IsNullOrWhiteSpace(compraVM.Busqueda))
            {
                string busqueda = compraVM.Busqueda.Trim();

                if (int.TryParse(busqueda, out int compraId))
                {
                    consulta = consulta.Where(c =>
                        c.Id == compraId ||
                        (c.NumeroComprobante != null && c.NumeroComprobante.Contains(busqueda)) ||
                        (c.TipoComprobante != null && c.TipoComprobante.Contains(busqueda)));
                }
                else
                {
                    consulta = consulta.Where(c =>
                        (c.NumeroComprobante != null && c.NumeroComprobante.Contains(busqueda)) ||
                        (c.TipoComprobante != null && c.TipoComprobante.Contains(busqueda)));
                }
            }

            compraVM.Compras = await consulta
                .OrderByDescending(c => c.Fecha)
                .ThenByDescending(c => c.Id)
                .Select(c => new CompraItemVM
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    ProveedorNombre = c.Proveedor.RazonSocial,
                    TipoComprobante = c.TipoComprobante,
                    NumeroComprobante = c.NumeroComprobante,
                    Total = c.Total,
                    Estado = c.Estado,
                    EmpresaNombre = c.Empresa.Nombre
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                compraVM.Empresas = await _context.Empresas
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

            IQueryable<Proveedor> proveedoresConsulta = _context.Proveedores
                .AsNoTracking()
                .Where(p => p.Estado);

            if (!esSuperAdmin)
            {
                proveedoresConsulta = proveedoresConsulta.Where(p =>
                    p.EmpresaId == usuario.EmpresaId);
            }
            else if (compraVM.EmpresaId.HasValue)
            {
                proveedoresConsulta = proveedoresConsulta.Where(p =>
                    p.EmpresaId == compraVM.EmpresaId.Value);
            }

            compraVM.Proveedores = await proveedoresConsulta
                .OrderBy(p => p.RazonSocial)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.RazonSocial
                })
                .ToListAsync();

            return View(compraVM);
        }
        // GET: Compra/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            int empresaCompraId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    var compraVM = new CompraCreateVM();

                    await CargarEmpresas(compraVM);

                    return View(compraVM);
                }

                empresaCompraId = empresaId.Value;
            }
            else
            {
                empresaCompraId = usuario.EmpresaId;
            }

            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == empresaCompraId &&
                    e.Estado);

            if (empresa == null)
            {
                return NotFound();
            }

            var compraVMFinal = new CompraCreateVM
            {
                EmpresaId = esSuperAdmin ? empresaCompraId : null,
                Detalles = new List<DetalleCompraCreateVM>()
            };

            await PrepararCompraParaVista(
                compraVMFinal,
                empresaCompraId,
                esSuperAdmin);

            return View(compraVMFinal);
        }
        // GET: Compra/ObtenerProducto
        [HttpGet]
        public async Task<IActionResult> ObtenerProducto(int id, int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            int empresaCompraId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    return BadRequest();
                }

                empresaCompraId = empresaId.Value;
            }
            else
            {
                empresaCompraId = usuario.EmpresaId;
            }

            var producto = await _context.Productos
                .AsNoTracking()
                .Where(p =>
                    p.Id == id &&
                    p.EmpresaId == empresaCompraId &&
                    p.Estado)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.PrecioVenta
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound();
            }

            return Json(new
            {
                producto.Id,
                producto.Nombre,
                producto.PrecioVenta
            });
        }
        // POST: Compra/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraCreateVM compraVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            int empresaCompraId;

            if (esSuperAdmin)
            {
                if (!compraVM.EmpresaId.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(compraVM.EmpresaId),
                        "Debe seleccionar una empresa.");

                    await CargarEmpresas(compraVM);

                    return View(compraVM);
                }

                empresaCompraId = compraVM.EmpresaId.Value;
            }
            else
            {
                empresaCompraId = usuario.EmpresaId;
                compraVM.EmpresaId = null;

                ModelState.Remove(nameof(compraVM.EmpresaId));
            }

            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == empresaCompraId &&
                    e.Estado);

            if (empresa == null)
            {
                return NotFound();
            }

            compraVM.TipoComprobante = NormalizarTextoOpcional(compraVM.TipoComprobante);
            compraVM.NumeroComprobante = NormalizarTextoOpcional(compraVM.NumeroComprobante);
            compraVM.Observaciones = NormalizarTextoOpcional(compraVM.Observaciones);

            if (compraVM.Detalles == null || compraVM.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(compraVM.Detalles),
                    "Debe agregar al menos un producto a la compra.");
            }

            if (compraVM.Detalles != null &&
                compraVM.Detalles.Any(d =>
                    d.ProductoId <= 0 ||
                    d.Cantidad <= 0 ||
                    d.PrecioUnitario < 0 ||
                    (d.NuevoPrecioVenta.HasValue && d.NuevoPrecioVenta.Value < 0)))
            {
                ModelState.AddModelError(
                    nameof(compraVM.Detalles),
                    "La compra contiene productos, cantidades o precios inválidos.");
            }

            if (!ModelState.IsValid)
            {
                await PrepararCompraParaVista(
                    compraVM,
                    empresaCompraId,
                    esSuperAdmin);

                return View(compraVM);
            }

            bool hayProductosRepetidos = compraVM.Detalles
                .GroupBy(d => d.ProductoId)
                .Any(g => g.Count() > 1);

            if (hayProductosRepetidos)
            {
                ModelState.AddModelError(
                    nameof(compraVM.Detalles),
                    "Un mismo producto no puede aparecer más de una vez en la compra.");

                await PrepararCompraParaVista(
                    compraVM,
                    empresaCompraId,
                    esSuperAdmin);

                return View(compraVM);
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(p =>
                        p.Id == compraVM.ProveedorId &&
                        p.EmpresaId == empresaCompraId &&
                        p.Estado);

                if (proveedor == null)
                {
                    ModelState.AddModelError(
                        nameof(compraVM.ProveedorId),
                        "El proveedor seleccionado no existe, se encuentra inactivo o no pertenece a la empresa.");

                    await transaccion.RollbackAsync();

                    await PrepararCompraParaVista(
                        compraVM,
                        empresaCompraId,
                        esSuperAdmin);

                    return View(compraVM);
                }

                if (!string.IsNullOrWhiteSpace(compraVM.NumeroComprobante))
                {
                    bool comprobanteDuplicado = await _context.Compras
                        .AsNoTracking()
                        .AnyAsync(c =>
                            c.EmpresaId == empresaCompraId &&
                            c.ProveedorId == proveedor.Id &&
                            c.TipoComprobante == compraVM.TipoComprobante &&
                            c.NumeroComprobante == compraVM.NumeroComprobante &&
                            c.Estado);

                    if (comprobanteDuplicado)
                    {
                        ModelState.AddModelError(
                            nameof(compraVM.NumeroComprobante),
                            "Ya existe una compra activa con ese comprobante para el proveedor.");

                        await transaccion.RollbackAsync();

                        await PrepararCompraParaVista(
                            compraVM,
                            empresaCompraId,
                            esSuperAdmin);

                        return View(compraVM);
                    }
                }

                var productosIds = compraVM.Detalles
                    .Select(d => d.ProductoId)
                    .ToList();

                var productos = await _context.Productos
                    .Where(p =>
                        productosIds.Contains(p.Id) &&
                        p.EmpresaId == empresaCompraId &&
                        p.Estado)
                    .ToListAsync();

                if (productos.Count != productosIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(compraVM.Detalles),
                        "Uno o más productos no existen, se encuentran inactivos o no pertenecen a la empresa.");

                    await transaccion.RollbackAsync();

                    await PrepararCompraParaVista(
                        compraVM,
                        empresaCompraId,
                        esSuperAdmin);

                    return View(compraVM);
                }

                var productosPorId = productos
                    .ToDictionary(p => p.Id);

                DateTime fechaCompra = DateTime.Now;
                decimal totalCompra = 0;

                var compra = new Compra
                {
                    Fecha = fechaCompra,
                    Total = 0,
                    Estado = true,
                    TipoComprobante = compraVM.TipoComprobante,
                    NumeroComprobante = compraVM.NumeroComprobante,
                    Observaciones = compraVM.Observaciones,
                    EmpresaId = empresaCompraId,
                    ProveedorId = proveedor.Id,
                    UsuarioId = usuario.Id
                };

                foreach (var detalleVM in compraVM.Detalles)
                {
                    var producto = productosPorId[detalleVM.ProductoId];

                    decimal subtotal =
                        detalleVM.Cantidad * detalleVM.PrecioUnitario;

                    decimal? precioVentaAnterior = null;
                    decimal? precioVentaNuevo = null;

                    if (detalleVM.NuevoPrecioVenta.HasValue &&
                        detalleVM.NuevoPrecioVenta.Value != producto.PrecioVenta)
                    {
                        precioVentaAnterior = producto.PrecioVenta;
                        precioVentaNuevo = detalleVM.NuevoPrecioVenta.Value;
                    }

                    compra.Detalles.Add(new DetalleCompra
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalleVM.Cantidad,
                        PrecioUnitario = detalleVM.PrecioUnitario,
                        Subtotal = subtotal,
                        PrecioCostoAnterior = producto.PrecioCosto,
                        PrecioVentaAnterior = precioVentaAnterior,
                        PrecioVentaNuevo = precioVentaNuevo
                    });

                    int stockAnterior = producto.Stock;
                    int stockPosterior =
                        stockAnterior + detalleVM.Cantidad;

                    producto.Stock = stockPosterior;

                    producto.PrecioCosto =
                        detalleVM.PrecioUnitario;

                    if (precioVentaNuevo.HasValue)
                    {
                        producto.PrecioVenta =
                            precioVentaNuevo.Value;
                    }

                    compra.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        EmpresaId = empresaCompraId,
                        Tipo = TipoMovimientoStock.Compra,
                        Cantidad = detalleVM.Cantidad,
                        StockAnterior = stockAnterior,
                        StockPosterior = stockPosterior,
                        Fecha = fechaCompra,
                        UsuarioId = usuario.Id
                    });

                    totalCompra += subtotal;
                }

                compra.Total = totalCompra;

                _context.Compras.Add(compra);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Compra registrada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = compra.Id });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible registrar la compra debido a un error en la base de datos.");

                await PrepararCompraParaVista(
                    compraVM,
                    empresaCompraId,
                    esSuperAdmin);

                return View(compraVM);
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error inesperado al registrar la compra.");

                await PrepararCompraParaVista(
                    compraVM,
                    empresaCompraId,
                    esSuperAdmin);

                return View(compraVM);
            }
        }
        // GET: Compra/Details/5
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

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Compra> consulta = _context.Compras
                .AsNoTracking()
                .Include(c => c.Empresa)
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.UsuarioAnulacion)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            var compra = await consulta
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
            {
                return NotFound();
            }

            decimal totalPagado =
                await _compraSaldoService.ObtenerTotalPagado(
                    compra.Id);

            decimal saldoPendiente =
                await _compraSaldoService.ObtenerSaldoPendiente(
                    compra.Id,
                    compra.Total);

            var pagos =
                await _context.PagosProveedor
                    .AsNoTracking()
                    .Where(p =>
                        p.CompraId == compra.Id &&
                        p.EmpresaId == compra.EmpresaId)
                    .OrderByDescending(p => p.Fecha)
                    .ThenByDescending(p => p.Id)
                    .Select(p => new PagoProveedorResumenVM
                    {
                        Id = p.Id,
                        Fecha = p.Fecha,
                        Importe = p.Importe,
                        MedioPagoNombre = p.MedioPago.Nombre,
                        CajaNombre = p.Caja.Nombre,
                        UsuarioNombre = p.Usuario.Email ?? string.Empty,
                        Estado = p.Estado,
                        TurnoCajaId = p.TurnoCajaId,
                        FechaAnulacion = p.FechaAnulacion,
                        UsuarioAnulacionNombre =
                            p.UsuarioAnulacion != null
                                ? p.UsuarioAnulacion.Email
                                : null,
                        MotivoAnulacion = p.MotivoAnulacion
                    })
                    .ToListAsync();

            var compraVM = new CompraDetailsVM
            {
                Id = compra.Id,
                Fecha = compra.Fecha,
                ProveedorNombre = compra.Proveedor.RazonSocial,
                TipoComprobante = compra.TipoComprobante,
                NumeroComprobante = compra.NumeroComprobante,
                Total = compra.Total,
                TotalPagado = totalPagado,
                SaldoPendiente = saldoPendiente,
                Pagos = pagos,
                Estado = compra.Estado,
                Observaciones = compra.Observaciones,
                UsuarioEmail = compra.Usuario.Email ?? string.Empty,
                FechaAnulacion = compra.FechaAnulacion,
                UsuarioAnulacionEmail = compra.UsuarioAnulacion?.Email,
                EmpresaNombre = compra.Empresa.Nombre,
                Detalles = compra.Detalles
                    .Select(d => new DetalleCompraDetailsVM
                    {
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        PrecioVentaAnterior = d.PrecioVentaAnterior,
                        PrecioVentaNuevo = d.PrecioVentaNuevo
                    })
                    .ToList()
            };

            return View(compraVM);
        }
        // POST: Compra/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                IQueryable<Compra> consulta = _context.Compras
                    .Include(c => c.Detalles)
                        .ThenInclude(d => d.Producto);

                if (!esSuperAdmin)
                {
                    consulta = consulta.Where(c =>
                        c.EmpresaId == usuario.EmpresaId);
                }

                var compra = await consulta
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (compra == null)
                {
                    await transaccion.RollbackAsync();
                    return NotFound();
                }

                if (!compra.Estado)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] = "La compra ya se encuentra anulada.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }

                foreach (var detalle in compra.Detalles)
                {
                    if (detalle.Producto.Stock < detalle.Cantidad)
                    {
                        await transaccion.RollbackAsync();

                        TempData["Error"] =
                            $"No se puede anular la compra porque el producto \"{detalle.Producto.Nombre}\" " +
                            $"tiene stock insuficiente. Disponible: {detalle.Producto.Stock}. " +
                            $"Debe retirarse: {detalle.Cantidad}.";

                        return RedirectToAction(
                            nameof(Details),
                            new { id });
                    }
                }

                DateTime fechaAnulacion = DateTime.Now;

                foreach (var detalle in compra.Detalles)
                {
                    var producto = detalle.Producto;

                    int stockAnterior = producto.Stock;
                    int stockPosterior = stockAnterior - detalle.Cantidad;

                    producto.Stock = stockPosterior;

                    bool existeCompraCostoPosterior =
                        await _context.DetallesCompra
                            .AsNoTracking()
                            .AnyAsync(d =>
                                d.ProductoId == producto.Id &&
                                d.CompraId != compra.Id &&
                                d.Compra.Estado &&
                                (
                                    d.Compra.Fecha > compra.Fecha ||
                                    (d.Compra.Fecha == compra.Fecha &&
                                    d.CompraId > compra.Id)
                                ));

                    if (!existeCompraCostoPosterior &&
                        producto.PrecioCosto == detalle.PrecioUnitario)
                    {
                        producto.PrecioCosto =
                            detalle.PrecioCostoAnterior;
                    }

                    if (detalle.PrecioVentaAnterior.HasValue &&
                        detalle.PrecioVentaNuevo.HasValue)
                    {
                        bool existeCambioVentaPosterior =
                            await _context.DetallesCompra
                                .AsNoTracking()
                                .AnyAsync(d =>
                                    d.ProductoId == producto.Id &&
                                    d.CompraId != compra.Id &&
                                    d.Compra.Estado &&
                                    d.PrecioVentaNuevo.HasValue &&
                                    (
                                        d.Compra.Fecha > compra.Fecha ||
                                        (d.Compra.Fecha == compra.Fecha &&
                                        d.CompraId > compra.Id)
                                    ));

                        if (!existeCambioVentaPosterior &&
                            producto.PrecioVenta == detalle.PrecioVentaNuevo.Value)
                        {
                            producto.PrecioVenta =
                                detalle.PrecioVentaAnterior.Value;
                        }
                    }

                    compra.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        EmpresaId = compra.EmpresaId,
                        Tipo = TipoMovimientoStock.AnulacionCompra,
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockPosterior = stockPosterior,
                        Fecha = fechaAnulacion,
                        UsuarioId = usuario.Id
                    });
                }

                compra.Estado = false;
                compra.FechaAnulacion = fechaAnulacion;
                compra.UsuarioAnulacionId = usuario.Id;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Success"] = "Compra anulada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = compra.Id });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                TempData["Error"] =
                    "No fue posible anular la compra debido a un error en la base de datos.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                TempData["Error"] =
                    "Ocurrió un error inesperado al anular la compra.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        //Helpers
        private async Task PrepararCompraParaVista(CompraCreateVM compraVM, int empresaId, bool esSuperAdmin)
        {
            compraVM.Proveedores = await _context.Proveedores
                .AsNoTracking()
                .Where(p =>
                    p.EmpresaId == empresaId &&
                    p.Estado)
                .OrderBy(p => p.RazonSocial)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.RazonSocial
                })
                .ToListAsync();

            compraVM.Productos = await _context.Productos
                .AsNoTracking()
                .Where(p =>
                    p.EmpresaId == empresaId &&
                    p.Estado)
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                compraVM.EmpresaId = empresaId;
                await CargarEmpresas(compraVM);
            }

            if (compraVM.Detalles != null && compraVM.Detalles.Count > 0)
            {
                var productosIds = compraVM.Detalles
                    .Select(d => d.ProductoId)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                var preciosVenta = await _context.Productos
                    .AsNoTracking()
                    .Where(p =>
                        productosIds.Contains(p.Id) &&
                        p.EmpresaId == empresaId)
                    .ToDictionaryAsync(
                        p => p.Id,
                        p => p.PrecioVenta);

                foreach (var detalle in compraVM.Detalles)
                {
                    if (preciosVenta.TryGetValue(
                        detalle.ProductoId,
                        out decimal precioVenta))
                    {
                        detalle.PrecioVentaActual = precioVenta;
                    }
                }
            }
        }
        private async Task CargarEmpresas(CompraCreateVM compraVM)
        {
            compraVM.Empresas = await _context.Empresas
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

        private static string? NormalizarTextoOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
    }
}