using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
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
        public IActionResult Index()
        {
            return View();
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
        public async Task<IActionResult> Create(
            VentaCreateVM ventaVM,
            int? empresaId = null)
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
                    var producto = productosPorId[
                        detalleVM.ProductoId];

                    if (producto.Stock < detalleVM.Cantidad)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.Detalles),
                            $"Stock insuficiente para \"{producto.Nombre}\". Disponible: {producto.Stock}. Solicitado: {detalleVM.Cantidad}.");

                        await transaccion.RollbackAsync();

                        await PrepararVentaParaVista(
                            ventaVM,
                            empresaVentaId);

                        return View(ventaVM);
                    }

                    decimal precioUnitario =
                        producto.PrecioVenta;

                    decimal subtotal =
                        precioUnitario * detalleVM.Cantidad;

                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalleVM.Cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = subtotal
                    });

                    producto.Stock -= detalleVM.Cantidad;

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
        public IActionResult Details(int id)
        {
            return View();
        }

        // GET: Venta/BuscarProductos
        [HttpGet]
        public async Task<IActionResult> BuscarProductos(
            string? termino,
            int? empresaId = null)
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
        public async Task<IActionResult> BuscarClientes(
            string? termino,
            int? empresaId = null)
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
        private async Task PrepararVentaParaVista(
    VentaCreateVM ventaVM,
    int empresaId)
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