using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class VentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public VentaController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Venta
        public async Task<IActionResult> Index(VentaIndexVM ventaVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            IQueryable<Venta> consulta = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Empresa)
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .Include(v => v.Detalles);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(v =>
                    v.EmpresaId == usuario.EmpresaId);
            }
            else if (ventaVM.EmpresaId.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.EmpresaId == ventaVM.EmpresaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(ventaVM.Buscar))
            {
                string buscar = ventaVM.Buscar.Trim();

                if (int.TryParse(buscar, out int ventaId))
                {
                    consulta = consulta.Where(v =>
                        v.Id == ventaId ||
                        (
                            v.Cliente != null &&
                            (
                                v.Cliente.Nombre.Contains(buscar) ||
                                (v.Cliente.Apellido != null &&
                                 v.Cliente.Apellido.Contains(buscar)) ||
                                (v.Cliente.Documento != null &&
                                 v.Cliente.Documento.Contains(buscar))
                            )
                        ));
                }
                else
                {
                    consulta = consulta.Where(v =>
                        v.Cliente != null &&
                        (
                            v.Cliente.Nombre.Contains(buscar) ||
                            (v.Cliente.Apellido != null &&
                             v.Cliente.Apellido.Contains(buscar)) ||
                            (v.Cliente.Documento != null &&
                             v.Cliente.Documento.Contains(buscar))
                        ));
                }
            }

            if (ventaVM.FechaDesde.HasValue)
            {
                DateTime fechaDesde = ventaVM.FechaDesde.Value.Date;

                consulta = consulta.Where(v =>
                    v.Fecha >= fechaDesde);
            }

            if (ventaVM.FechaHasta.HasValue)
            {
                DateTime fechaHasta =
                    ventaVM.FechaHasta.Value.Date.AddDays(1);

                consulta = consulta.Where(v =>
                    v.Fecha < fechaHasta);
            }

            if (!string.IsNullOrWhiteSpace(ventaVM.UsuarioId))
            {
                consulta = consulta.Where(v =>
                    v.UsuarioId == ventaVM.UsuarioId);
            }

            if (ventaVM.Estado.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.Estado == ventaVM.Estado.Value);
            }

            ventaVM.Ventas = await consulta
                .OrderByDescending(v => v.Fecha)
                .ThenByDescending(v => v.Id)
                .Select(v => new VentaIndexItemVM
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteNombre = v.Cliente == null
                        ? "Cliente ocasional"
                        : v.Cliente.Apellido == null
                            ? v.Cliente.Nombre
                            : v.Cliente.Nombre + " " + v.Cliente.Apellido,
                    UsuarioNombre =
                        v.Usuario.Nombre + " " + v.Usuario.Apellido,
                    EmpresaNombre = v.Empresa.Nombre,
                    Total = v.Total,
                    Estado = v.Estado,
                    TotalUnidades = v.Detalles.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                ventaVM.Empresas = await _context.Empresas
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

            IQueryable<Usuario> usuariosConsulta =
                _userManager.Users
                    .AsNoTracking()
                    .Where(u => u.Estado);

            if (!esSuperAdmin)
            {
                usuariosConsulta = usuariosConsulta.Where(u =>
                    u.EmpresaId == usuario.EmpresaId);
            }
            else if (ventaVM.EmpresaId.HasValue)
            {
                usuariosConsulta = usuariosConsulta.Where(u =>
                    u.EmpresaId == ventaVM.EmpresaId.Value);
            }

            ventaVM.Usuarios = await usuariosConsulta
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Nombre + " " + u.Apellido
                })
                .ToListAsync();

            return View(ventaVM);
        }
        // GET: Venta/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    TempData["Error"] = "Debe seleccionar una empresa para ingresar al Punto de Venta.";

                    return RedirectToAction("Index", "Empresa");
                }

                empresaVentaId = empresaId.Value;
            }
            else
            {
                empresaVentaId = usuario.EmpresaId;
            }

            var empresa = await _context.Empresas
        .AsNoTracking()
        .FirstOrDefaultAsync(e =>
            e.Id == empresaVentaId &&
            e.Estado);

            if (empresa == null)
            {
                return NotFound();
            }

            var ventaVM = new VentaCreateVM
            {
                ClienteId = null,
                ClienteNombre = "Cliente ocasional",
                Detalles = new List<VentaDetalleCreateVM>()
            };

            ViewBag.EmpresaId = empresa.Id;
            ViewBag.EmpresaNombre = empresa.Nombre;

            return View(ventaVM);
        }
        // POST: Venta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateVM ventaVM,int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    return BadRequest();
                }

                empresaVentaId = empresaId.Value;
            }
            else
            {
                empresaVentaId = usuario.EmpresaId;
            }

            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == empresaVentaId &&
                    e.Estado);

            if (empresa == null)
            {
                return NotFound();
            }

            ViewBag.EmpresaId = empresa.Id;
            ViewBag.EmpresaNombre = empresa.Nombre;

            if (ventaVM.Detalles == null ||
                ventaVM.Detalles.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(ventaVM.Detalles),
                    "Debe agregar al menos un producto a la venta.");

                await PrepararVentaParaVista(
                    ventaVM,
                    empresaVentaId);

                return View(ventaVM);
            }

            if (ventaVM.Detalles.Any(d =>
                d.ProductoId <= 0 ||
                d.Cantidad <= 0))
            {
                ModelState.AddModelError(
                    nameof(ventaVM.Detalles),
                    "La venta contiene productos o cantidades inválidas.");

                await PrepararVentaParaVista(
                    ventaVM,
                    empresaVentaId);

                return View(ventaVM);
            }

            /*
             * Aunque JavaScript evita productos repetidos, el POST puede
             * manipularse. El servidor agrupa las líneas repetidas y suma
             * sus cantidades antes de procesar la venta.
             */
            ventaVM.Detalles = ventaVM.Detalles
                .GroupBy(d => d.ProductoId)
                .Select(g => new VentaDetalleCreateVM
                {
                    ProductoId = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad)
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                await PrepararVentaParaVista(
                    ventaVM,
                    empresaVentaId);

                return View(ventaVM);
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                Cliente? cliente = null;

                if (ventaVM.ClienteId.HasValue)
                {
                    cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c =>
                            c.Id == ventaVM.ClienteId.Value &&
                            c.EmpresaId == empresaVentaId &&
                            c.Estado);

                    if (cliente == null)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.ClienteId),
                            "El cliente seleccionado no existe, se encuentra inactivo o no pertenece a la empresa.");

                        await transaccion.RollbackAsync();

                        await PrepararVentaParaVista(
                            ventaVM,
                            empresaVentaId);

                        return View(ventaVM);
                    }
                }

                var productosIds = ventaVM.Detalles
                    .Select(d => d.ProductoId)
                    .ToList();

                var productos = await _context.Productos
                    .Where(p =>
                        productosIds.Contains(p.Id) &&
                        p.EmpresaId == empresaVentaId &&
                        p.Estado)
                    .ToListAsync();

                if (productos.Count != productosIds.Count)
                {
                    ModelState.AddModelError(
                        nameof(ventaVM.Detalles),
                        "Uno o más productos no existen, se encuentran inactivos o no pertenecen a la empresa.");

                    await transaccion.RollbackAsync();

                    await PrepararVentaParaVista(
                        ventaVM,
                        empresaVentaId);

                    return View(ventaVM);
                }

                var productosPorId = productos
                    .ToDictionary(p => p.Id);

                decimal totalVenta = 0;

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    Total = 0,
                    Estado = true,
                    EmpresaId = empresaVentaId,
                    UsuarioId = usuario.Id,
                    ClienteId = cliente?.Id
                };

                foreach (var detalleVM in ventaVM.Detalles)
                {
                    var producto = productosPorId[detalleVM.ProductoId];

                    if (producto.Stock < detalleVM.Cantidad)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.Detalles),
                            $"Stock insuficiente para \"{producto.Nombre}\". Disponible: {producto.Stock}. Solicitado: {detalleVM.Cantidad}.");

                        await transaccion.RollbackAsync();
                        await PrepararVentaParaVista(ventaVM, empresaVentaId);

                        return View(ventaVM);
                    }

                    decimal precioUnitario = producto.PrecioVenta;
                    decimal subtotal = precioUnitario * detalleVM.Cantidad;

                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalleVM.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = subtotal
                    });

                    int stockAnterior = producto.Stock;
                    int stockPosterior = stockAnterior - detalleVM.Cantidad;

                    producto.Stock = stockPosterior;

                    venta.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        EmpresaId = empresaVentaId,
                        Tipo = TipoMovimientoStock.Venta,
                        Cantidad = detalleVM.Cantidad,
                        StockAnterior = stockAnterior,
                        StockPosterior = stockPosterior,
                        Fecha = venta.Fecha,
                        UsuarioId = usuario.Id
                    });

                    totalVenta += subtotal;
                }

                venta.Total = totalVenta;

                _context.Ventas.Add(venta);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Success"] =
                    "Venta registrada correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = venta.Id });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "No fue posible registrar la venta debido a un error en la base de datos.");

                await PrepararVentaParaVista(
                    ventaVM,
                    empresaVentaId);

                return View(ventaVM);
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error inesperado al registrar la venta.");

                await PrepararVentaParaVista(
                    ventaVM,
                    empresaVentaId);

                return View(ventaVM);
            }
        }
        // GET: Venta/Details/5
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

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            IQueryable<Venta> consulta = _context.Ventas
                .AsNoTracking()
                .Include(v => v.Empresa)
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(v =>
                    v.EmpresaId == usuario.EmpresaId);
            }

            var venta = await consulta
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            var ventaVM = new VentaDetailsVM
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                Total = venta.Total,
                Estado = venta.Estado,
                EmpresaId = venta.EmpresaId,
                EmpresaNombre = venta.Empresa.Nombre,
                UsuarioNombre = $"{venta.Usuario.Nombre} {venta.Usuario.Apellido}",
                ClienteNombre = venta.Cliente == null
                    ? "Cliente ocasional"
                    : string.IsNullOrWhiteSpace(venta.Cliente.Apellido)
                        ? venta.Cliente.Nombre
                        : $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}",
                ClienteDocumento = venta.Cliente?.Documento,
                ClienteEmail = venta.Cliente?.Email,
                Detalles = venta.Detalles
                    .Select(d => new VentaDetalleDetailsVM
                    {
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        CodigoBarra = d.Producto.CodigoBarra,
                        PrecioUnitario = d.PrecioUnitario,
                        Cantidad = d.Cantidad,
                        Subtotal = d.Subtotal
                    })
                    .ToList()
            };

            return View(ventaVM);
        }
        // POST: Venta/Anular/5
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

            await using var transaccion = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                IQueryable<Venta> consulta = _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto);

                if (!esSuperAdmin)
                {
                    consulta = consulta.Where(v => v.EmpresaId == usuario.EmpresaId);
                }

                var venta = await consulta.FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                {
                    await transaccion.RollbackAsync();
                    return NotFound();
                }

                if (!venta.Estado)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] = "La venta ya se encuentra anulada.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                DateTime fechaAnulacion = DateTime.Now;

                foreach (var detalle in venta.Detalles)
                {
                    int stockAnterior = detalle.Producto.Stock;
                    int stockPosterior = stockAnterior + detalle.Cantidad;

                    detalle.Producto.Stock = stockPosterior;

                    venta.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = detalle.ProductoId,
                        EmpresaId = venta.EmpresaId,
                        Tipo = TipoMovimientoStock.AnulacionVenta,
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockPosterior = stockPosterior,
                        Fecha = fechaAnulacion,
                        UsuarioId = usuario.Id
                    });
                }

                venta.Estado = false;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Success"] = "Venta anulada correctamente. El stock fue restaurado.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                TempData["Error"] = "No fue posible anular la venta debido a un error en la base de datos.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                TempData["Error"] = "Ocurrió un error inesperado al anular la venta.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
        // GET: Venta/BuscarProductos
        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string? termino, int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debe indicar una empresa."
                    });
                }

                empresaVentaId = empresaId.Value;
            }
            else
            {
                empresaVentaId = usuario.EmpresaId;
            }

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == empresaVentaId &&
                    e.Estado);

            if (!empresaValida)
            {
                return NotFound(new
                {
                    mensaje = "La empresa indicada no existe o se encuentra inactiva."
                });
            }

            if (string.IsNullOrWhiteSpace(termino))
            {
                return Json(Array.Empty<VentaProductoBusquedaVM>());
            }

            termino = termino.Trim();

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p =>
                    p.EmpresaId == empresaVentaId &&
                    p.Estado &&
                    (
                        p.Nombre.Contains(termino) ||
                        (p.CodigoBarra != null &&
                         p.CodigoBarra.Contains(termino))
                    ))
                .OrderBy(p => p.Nombre)
                .Take(10)
                .Select(p => new VentaProductoBusquedaVM
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarra = p.CodigoBarra,
                    PrecioVenta = p.PrecioVenta,
                    StockDisponible = p.Stock
                })
                .ToListAsync();

            return Json(productos);
        }
        // GET: Venta/BuscarClientes
        [HttpGet]
        public async Task<IActionResult> BuscarClientes(string? termino, int? empresaId = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    return BadRequest(new
                    {
                        mensaje = "Debe indicar una empresa."
                    });
                }

                empresaVentaId = empresaId.Value;
            }
            else
            {
                empresaVentaId = usuario.EmpresaId;
            }

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == empresaVentaId &&
                    e.Estado);

            if (!empresaValida)
            {
                return NotFound(new
                {
                    mensaje = "La empresa indicada no existe o se encuentra inactiva."
                });
            }

            if (string.IsNullOrWhiteSpace(termino))
            {
                return Json(Array.Empty<VentaClienteBusquedaVM>());
            }

            termino = termino.Trim();

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c =>
                    c.EmpresaId == empresaVentaId &&
                    c.Estado &&
                    (
                        c.Nombre.Contains(termino) ||
                        (c.Apellido != null &&
                         c.Apellido.Contains(termino)) ||
                        (c.Documento != null &&
                         c.Documento.Contains(termino)) ||
                        (c.Email != null &&
                         c.Email.Contains(termino))
                    ))
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Take(10)
                .Select(c => new VentaClienteBusquedaVM
                {
                    Id = c.Id,
                    NombreCompleto = c.Apellido == null
                        ? c.Nombre
                        : c.Nombre + " " + c.Apellido,
                    Documento = c.Documento,
                    Email = c.Email,
                    Telefono = c.Telefono
                })
                .ToListAsync();

            return Json(clientes);
        }
        private async Task PrepararVentaParaVista(VentaCreateVM ventaVM, int empresaId)
        {
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            ViewBag.EmpresaId = empresaId;
            ViewBag.EmpresaNombre =
                empresa?.Nombre ?? "Empresa";

            ventaVM.Detalles ??=
                new List<VentaDetalleCreateVM>();

            if (ventaVM.Detalles.Count > 0)
            {
                var productosIds = ventaVM.Detalles
                    .Select(d => d.ProductoId)
                    .Distinct()
                    .ToList();

                var productos = await _context.Productos
                    .AsNoTracking()
                    .Where(p =>
                        productosIds.Contains(p.Id) &&
                        p.EmpresaId == empresaId)
                    .ToDictionaryAsync(p => p.Id);

                ventaVM.Detalles = ventaVM.Detalles
                    .Where(d =>
                        productos.ContainsKey(d.ProductoId))
                    .Select(d =>
                    {
                        var producto =
                            productos[d.ProductoId];

                        return new VentaDetalleCreateVM
                        {
                            ProductoId = producto.Id,
                            ProductoNombre = producto.Nombre,
                            CodigoBarra = producto.CodigoBarra,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = producto.PrecioVenta,
                            StockDisponible = producto.Stock,
                            Subtotal =
                                producto.PrecioVenta * d.Cantidad,
                            StockSuficiente =
                                d.Cantidad > 0 &&
                                d.Cantidad <= producto.Stock
                        };
                    })
                    .ToList();
            }

            if (ventaVM.ClienteId.HasValue)
            {
                var cliente = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == ventaVM.ClienteId.Value &&
                        c.EmpresaId == empresaId &&
                        c.Estado);

                if (cliente != null)
                {
                    ventaVM.ClienteNombre =
                        string.IsNullOrWhiteSpace(cliente.Apellido)
                            ? cliente.Nombre
                            : $"{cliente.Nombre} {cliente.Apellido}";
                }
                else
                {
                    ventaVM.ClienteId = null;
                    ventaVM.ClienteNombre =
                        "Cliente ocasional";
                }
            }
            else
            {
                ventaVM.ClienteNombre =
                    "Cliente ocasional";
            }
        }
    }
}