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
    public class VentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly VentaSaldoService _ventaSaldoService;

        public VentaController(SaasDbContext context, UserManager<Usuario> userManager, VentaSaldoService ventaSaldoService)
        {
            _context = context;
            _userManager = userManager;
            _ventaSaldoService = ventaSaldoService;
        }

        // GET: Venta
        public async Task<IActionResult> Index(VentaIndexVM ventaVM, int pagina = 1)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            if (ventaVM.FechaDesde.HasValue && ventaVM.FechaHasta.HasValue && ventaVM.FechaHasta.Value.Date < ventaVM.FechaDesde.Value.Date)
            {
                ModelState.AddModelError(nameof(ventaVM.FechaHasta), "La fecha Hasta no puede ser anterior a la fecha Desde.");
                await CargarFiltrosIndexAsync(ventaVM, usuario, esSuperAdmin);
                ViewBag.PaginaActual = 1;
                ViewBag.TotalPaginas = 0;
                ViewBag.TotalRegistros = 0;
                return View(ventaVM);
            }

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

            if (ventaVM.EstadoCobro.HasValue)
            {
                switch (ventaVM.EstadoCobro.Value)
                {
                    case EstadoCobroVentaFiltro.Cobrada:

                        consulta =
                            consulta.Where(v =>
                                (
                                    v.CobrosVenta
                                        .Where(c =>
                                            c.Estado ==
                                            EstadoCobro.Activo)
                                        .Sum(c =>
                                            (decimal?)c.Importe)
                                    ?? 0
                                ) >= v.Total);

                        break;

                    case EstadoCobroVentaFiltro.PagoParcial:

                        consulta =
                            consulta.Where(v =>
                                (
                                    v.CobrosVenta
                                        .Where(c =>
                                            c.Estado ==
                                            EstadoCobro.Activo)
                                        .Sum(c =>
                                            (decimal?)c.Importe)
                                    ?? 0
                                ) > 0
                                &&
                                (
                                    v.CobrosVenta
                                        .Where(c =>
                                            c.Estado ==
                                            EstadoCobro.Activo)
                                        .Sum(c =>
                                            (decimal?)c.Importe)
                                    ?? 0
                                ) < v.Total);

                        break;

                    case EstadoCobroVentaFiltro.Pendiente:

                        consulta =
                            consulta.Where(v =>
                                (
                                    v.CobrosVenta
                                        .Where(c =>
                                            c.Estado ==
                                            EstadoCobro.Activo)
                                        .Sum(c =>
                                            (decimal?)c.Importe)
                                    ?? 0
                                ) <= 0);

                        break;
                }
            }

            var resumenPendiente = await consulta
                .Where(v => v.Estado)
                .Select(v => new
                {
                    Saldo = v.Total - (v.CobrosVenta.Where(c => c.Estado == EstadoCobro.Activo).Sum(c => (decimal?)c.Importe) ?? 0)
                })
                .Where(v => v.Saldo > 0)
                .GroupBy(v => 1)
                .Select(g => new { Cantidad = g.Count(), Total = g.Sum(v => v.Saldo) })
                .FirstOrDefaultAsync();

            ventaVM.CantidadConSaldoPendiente = resumenPendiente?.Cantidad ?? 0;
            ventaVM.TotalPendienteCobro = resumenPendiente?.Total ?? 0;

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalVentas = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalVentas / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalVentas;

            ventaVM.Ventas = await consulta
                .OrderByDescending(v => v.Fecha)
                .ThenByDescending(v => v.Id)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(v => new VentaIndexItemVM
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    ClienteNombre = v.Cliente == null
                        ? "Consumidor Final"
                        : v.Cliente.Apellido == null
                            ? v.Cliente.Nombre
                            : v.Cliente.Nombre + " " + v.Cliente.Apellido,
                    UsuarioNombre =
                        v.Usuario.Nombre + " " + v.Usuario.Apellido,
                    EmpresaNombre = v.Empresa.Nombre,
                    Total = v.Total,
                    TotalCobrado =
                        v.CobrosVenta
                            .Where(c =>
                                c.Estado == EstadoCobro.Activo)
                            .Sum(c =>
                                (decimal?)c.Importe)
                        ?? 0,
                    Estado = v.Estado,
                    TotalUnidades = v.Detalles.Sum(d => d.Cantidad)
                })
                .ToListAsync();

            await CargarFiltrosIndexAsync(ventaVM, usuario, esSuperAdmin);

            return View(ventaVM);
        }

        // Conserva las opciones de los filtros cuando el rango de fechas no es válido.
        private async Task CargarFiltrosIndexAsync(VentaIndexVM ventaVM, Usuario usuario, bool esSuperAdmin)
        {
            if (esSuperAdmin)
            {
                ventaVM.Empresas = await _context.Empresas.AsNoTracking().Where(e => e.Estado).OrderBy(e => e.Nombre).Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre
                }).ToListAsync();
            }

            IQueryable<Usuario> usuariosConsulta = _userManager.Users.AsNoTracking().Where(u => u.Estado);

            if (!esSuperAdmin)
            {
                usuariosConsulta = usuariosConsulta.Where(u => u.EmpresaId == usuario.EmpresaId);
            }
            else if (ventaVM.EmpresaId.HasValue)
            {
                usuariosConsulta = usuariosConsulta.Where(u => u.EmpresaId == ventaVM.EmpresaId.Value);
            }

            ventaVM.Usuarios = await usuariosConsulta.OrderBy(u => u.Nombre).ThenBy(u => u.Apellido).Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Nombre + " " + u.Apellido
            }).ToListAsync();
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
                ClienteNombre = "Consumidor Final",
                Detalles = new List<VentaDetalleCreateVM>(),
                Pagos = new List<VentaPagoCreateVM>()
            };

            await PrepararVentaParaVista(
                ventaVM,
                empresaVentaId);

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

            var turnosPorPago =  new Dictionary<int, int?>();

            var turnoOperativo =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.EmpresaId == empresaVentaId &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

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

                foreach (var detalleVM in ventaVM.Detalles)
                {
                    var producto =
                        productosPorId[detalleVM.ProductoId];

                    if (producto.Stock < detalleVM.Cantidad)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.Detalles),
                            $"Stock insuficiente para \"{producto.Nombre}\". " +
                            $"Disponible: {producto.Stock}. " +
                            $"Solicitado: {detalleVM.Cantidad}.");

                        await transaccion.RollbackAsync();

                        await PrepararVentaParaVista(
                            ventaVM,
                            empresaVentaId);

                        return View(ventaVM);
                    }

                    totalVenta +=
                        producto.PrecioVenta *
                        detalleVM.Cantidad;
                }

                ventaVM.Pagos ??= new List<VentaPagoCreateVM>();

                var pagos =
                    ventaVM.Pagos
                        .Where(p =>
                            p.MedioPagoId > 0 ||
                            p.CajaId > 0 ||
                            p.Importe > 0)
                        .ToList();

                ventaVM.Pagos = pagos;



                for (int i = 0; i < pagos.Count; i++)
                {
                    var pago = pagos[i];

                    if (pago.Importe <= 0)
                    {
                        ModelState.AddModelError(
                            $"Pagos[{i}].Importe",
                            "El importe debe ser mayor a 0.");

                        continue;
                    }

                    var caja = await _context.Cajas
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c =>
                            c.Id == pago.CajaId &&
                            c.EmpresaId == empresaVentaId &&
                            c.Estado);

                    if (caja == null)
                    {
                        ModelState.AddModelError(
                            $"Pagos[{i}].CajaId",
                            "La caja seleccionada no es válida.");

                        continue;
                    }

                    var medioPago =
                        await _context.CajaMediosPago
                            .AsNoTracking()
                            .Where(cm =>
                                cm.CajaId == pago.CajaId &&
                                cm.MedioPagoId == pago.MedioPagoId &&
                                cm.Caja.EmpresaId == empresaVentaId &&
                                cm.Caja.Estado &&
                                cm.MedioPago.EmpresaId == empresaVentaId &&
                                cm.MedioPago.Estado)
                            .Select(cm => new
                            {
                                cm.MedioPago.Tipo
                            })
                            .FirstOrDefaultAsync();

                    if (medioPago == null)
                    {
                        ModelState.AddModelError(
                            $"Pagos[{i}].MedioPagoId",
                            "El medio de pago no es válido para la caja seleccionada.");

                        continue;
                    }

                    if (medioPago.Tipo == TipoMedioPago.Efectivo &&
                        (!pago.ImporteRecibido.HasValue ||
                         pago.ImporteRecibido.Value < pago.Importe))
                    {
                        ModelState.AddModelError(
                            $"Pagos[{i}].ImporteRecibido",
                            "El importe recibido en efectivo debe ser igual o mayor al importe del pago.");

                        continue;
                    }

                    int? turnoMovimientoCajaId = null;

                    if (caja.PermiteTurnos)
                    {
                        if (turnoOperativo == null ||
                            turnoOperativo.CajaId != caja.Id)
                        {
                            ModelState.AddModelError(
                                $"Pagos[{i}].CajaId",
                                $"Debe tener un turno abierto propio para operar la caja \"{caja.Nombre}\".");

                            continue;
                        }

                        turnoMovimientoCajaId =
                            turnoOperativo.Id;
                    }

                    turnosPorPago[i] =
                        turnoMovimientoCajaId;
                }

                /*
                 * Los totales se evalúan después de validar cada línea. Así,
                 * un pago incompleto nunca habilita una venta ni cubre saldo.
                 */
                decimal totalPagado = pagos.Sum(p => p.Importe);

                if (ModelState.IsValid)
                {
                    if (totalPagado > totalVenta)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.Pagos),
                            "El total pagado no puede superar el total de la venta.");
                    }

                    if (totalPagado < totalVenta && !ventaVM.ClienteId.HasValue)
                    {
                        ModelState.AddModelError(
                            nameof(ventaVM.ClienteId),
                            "Debe seleccionar un cliente para dejar saldo pendiente.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await transaccion.RollbackAsync();

                    await PrepararVentaParaVista(
                        ventaVM,
                        empresaVentaId);

                    return View(ventaVM);
                }

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    Total = totalVenta,
                    Estado = true,
                    EmpresaId = empresaVentaId,
                    UsuarioId = usuario.Id,
                    ClienteId = cliente?.Id
                };

                foreach (var detalleVM in ventaVM.Detalles)
                {
                    var producto =
                        productosPorId[detalleVM.ProductoId];

                    decimal precioUnitario =
                        producto.PrecioVenta;

                    decimal subtotal =
                        precioUnitario *
                        detalleVM.Cantidad;

                    venta.Detalles.Add(
                        new DetalleVenta
                        {
                            ProductoId = producto.Id,
                            Cantidad = detalleVM.Cantidad,
                            PrecioUnitario = precioUnitario,
                            Subtotal = subtotal
                        });

                    int stockAnterior =
                        producto.Stock;

                    int stockPosterior =
                        stockAnterior -
                        detalleVM.Cantidad;

                    producto.Stock =
                        stockPosterior;

                    venta.MovimientosStock.Add(
                        new MovimientoStock
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
                }

                _context.Ventas.Add(venta);

                await _context.SaveChangesAsync();

                var cobros = new List<CobroVenta>();

                for (int i = 0; i < pagos.Count; i++)
                {
                    var pago =
                        pagos[i];

                    var cobro =
                        new CobroVenta
                        {
                            VentaId =
                                venta.Id,

                            EmpresaId =
                                empresaVentaId,

                            CajaId =
                                pago.CajaId,

                            MedioPagoId =
                                pago.MedioPagoId,

                            TurnoCajaId = turnosPorPago[i],

                            UsuarioId =
                                usuario.Id,

                            Fecha =
                                venta.Fecha,

                            Importe =
                                pago.Importe,

                            Estado =
                                EstadoCobro.Activo,

                            FechaAnulacion =
                                null,

                            UsuarioAnulacionId =
                                null,

                            MotivoAnulacion =
                                null
                        };

                    cobros.Add(cobro);
                }

                _context.CobrosVenta.AddRange(cobros);

                await _context.SaveChangesAsync();

                var movimientosCaja =
    new List<MovimientoCaja>();

                for (int i = 0; i < cobros.Count; i++)
                {
                    var cobro =
                        cobros[i];

                    var movimiento =
                        new MovimientoCaja
                        {
                            EmpresaId =
                                cobro.EmpresaId,

                            CajaId =
                                cobro.CajaId,

                            Tipo =
                                TipoMovimientoCaja.CobroVenta,

                            Direccion =
                                DireccionMovimientoCaja.Ingreso,

                            Importe =
                                cobro.Importe,

                            Fecha =
                                cobro.Fecha,

                            UsuarioId =
                                usuario.Id,

                            MedioPagoId =
                                cobro.MedioPagoId,

                            // Solo impacta en el arqueo cuando
                            // la caja del pago trabaja con ese turno.
                            TurnoCajaId =
                                turnosPorPago[i],

                            CategoriaGastoId =
                                null,

                            Concepto =
                                $"Cobro de venta #{venta.Id}",

                            Observaciones =
                                null,

                            CobroVentaId =
                                cobro.Id
                        };

                    movimientosCaja.Add(
                        movimiento);
                }

                _context.MovimientosCaja.AddRange(movimientosCaja);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    totalPagado == totalVenta
                        ? "Venta registrada y cobrada correctamente."
                        : totalPagado == 0
                            ? "Venta registrada a cuenta correctamente."
                            : "Venta registrada con saldo pendiente correctamente.";

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

            var cobros =
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.VentaId == venta.Id)
                    .OrderBy(c => c.Fecha)
                    .Select(c =>
                        new CobroVentaResumenVM
                        {
                            Id = c.Id,
                            Fecha = c.Fecha,
                            Importe = c.Importe,

                            MedioPagoNombre =
                                c.MedioPago.Nombre,

                            CajaNombre =
                                c.Caja.Nombre,

                            UsuarioNombre =
                                c.Usuario.UserName ?? "",

                            TurnoCajaId =
                                c.TurnoCajaId,

                            Estado =
                                c.Estado
                        })
                    .ToListAsync();

            decimal totalCobrado =
                await _ventaSaldoService
                    .ObtenerTotalCobrado(
                        venta.Id);

            var reintegros =
    await _context.ReintegrosVenta
        .AsNoTracking()
        .Where(r =>
            r.VentaId == venta.Id)
        .OrderBy(r =>
            r.Fecha)
        .Select(r =>
            new ReintegroVentaResumenVM
            {
                Id =
                    r.Id,

                Fecha =
                    r.Fecha,

                Importe =
                    r.Importe,

                MedioPagoNombre =
                    r.MedioPago.Nombre,

                CajaNombre =
                    r.Caja.Nombre,

                UsuarioNombre =
                    r.Usuario.UserName ?? "",

                Estado =
                    r.Estado
            })
        .ToListAsync();

            decimal totalReintegrado =
                await _ventaSaldoService
                    .ObtenerTotalReintegrado(
                        venta.Id);

            decimal pendienteReintegrar =
                await _ventaSaldoService
                    .ObtenerImporteDisponibleReintegro(
                        venta.Id);

            var ventaVM = new VentaDetailsVM
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                Total = venta.Total,
                Estado = venta.Estado,
                EmpresaId = venta.EmpresaId,
                EmpresaNombre = venta.Empresa.Nombre,

                UsuarioNombre =
        $"{venta.Usuario.Nombre} {venta.Usuario.Apellido}",

                ClienteNombre =
        venta.Cliente == null
            ? "Consumidor Final"
            : string.IsNullOrWhiteSpace(
                venta.Cliente.Apellido)
                ? venta.Cliente.Nombre
                : $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}",

                ClienteDocumento =
        venta.Cliente?.Documento,

                ClienteEmail =
        venta.Cliente?.Email,

                Detalles =
        venta.Detalles
            .Select(d =>
                new VentaDetalleDetailsVM
                {
                    ProductoId =
                        d.ProductoId,

                    ProductoNombre =
                        d.Producto.Nombre,

                    CodigoBarra =
                        d.Producto.CodigoBarra,

                    PrecioUnitario =
                        d.PrecioUnitario,

                    Cantidad =
                        d.Cantidad,

                    Subtotal =
                        d.Subtotal
                })
            .ToList(),

                TotalCobrado = totalCobrado,

                Cobros = cobros,

                TotalReintegrado = totalReintegrado,

                PendienteReintegrar = pendienteReintegrar,

                Reintegros = reintegros
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

                bool tieneCobrosActivos = await _context.CobrosVenta
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.VentaId == venta.Id &&
                        c.EmpresaId == venta.EmpresaId &&
                        c.Estado == EstadoCobro.Activo);

                if (tieneCobrosActivos)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] = "No se puede anular la venta porque tiene cobros activos. Debe anular primero los cobros asociados.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                bool tieneReintegrosActivos = await _context.ReintegrosVenta
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.VentaId == venta.Id &&
                        r.EmpresaId == venta.EmpresaId &&
                        r.Estado == EstadoReintegro.Activo);

                if (tieneReintegrosActivos)
                {
                    await transaccion.RollbackAsync();

                    TempData["Error"] = "No se puede anular la venta porque tiene reintegros activos. Debe anular primero los reintegros asociados.";

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
        [HttpGet]
        public async Task<IActionResult> GetMediosPagoPorCaja(int cajaId, int? empresaId = null)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            var caja =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == cajaId &&
                        c.Estado);

            if (caja == null)
            {
                return NotFound();
            }

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue ||
                    caja.EmpresaId != empresaId.Value)
                {
                    return Forbid();
                }

                empresaVentaId =
                    empresaId.Value;
            }
            else
            {
                if (caja.EmpresaId != usuario.EmpresaId)
                {
                    return Forbid();
                }

                empresaVentaId =
                    usuario.EmpresaId;
            }

            var medios =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm =>
                        cm.CajaId == caja.Id &&
                        cm.Caja.EmpresaId == empresaVentaId &&
                        cm.MedioPago.Estado)
                    .OrderBy(cm =>
                        cm.MedioPago.Nombre)
                    .Select(cm => new
                    {
                        id = cm.MedioPagoId,
                        nombre = cm.MedioPago.Nombre,
                        tipo = cm.MedioPago.Tipo
                    })
                    .ToListAsync();

            return Json(medios);
        }
        [HttpGet]
        public async Task<IActionResult> GetCajasPorMedioPago(int medioPagoId, int? empresaId = null)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            int empresaVentaId;

            if (esSuperAdmin)
            {
                if (!empresaId.HasValue)
                {
                    return BadRequest();
                }

                empresaVentaId =
                    empresaId.Value;
            }
            else
            {
                empresaVentaId =
                    usuario.EmpresaId;
            }

            var cajas =
                await _context.CajaMediosPago
                    .AsNoTracking()
                    .Where(cm =>
                        cm.MedioPagoId == medioPagoId &&
                        cm.MedioPago.EmpresaId == empresaVentaId &&
                        cm.MedioPago.Estado &&
                        cm.Caja.EmpresaId == empresaVentaId &&
                        cm.Caja.Estado)
                    .OrderBy(cm =>
                        cm.Caja.Nombre)
                    .Select(cm => new
                    {
                        id = cm.CajaId,
                        nombre = cm.Caja.Nombre
                    })
                    .ToListAsync();

            return Json(cajas);
        }

        //Helpers Methods
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
                        "Consumidor Final";
                }
            }
            else
            {
                ventaVM.ClienteNombre =
                    "Consumidor Final";
            }
            ventaVM.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CajaOpcionSimpleVM
                    {
                        Id = c.Id,
                        Nombre = c.Nombre
                    })
                    .ToListAsync();

            ventaVM.MediosPagoDisponibles =
                await _context.MediosPago
                    .AsNoTracking()
                    .Where(m =>
                        m.EmpresaId == empresaId &&
                        m.Estado)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new MedioPagoOpcionSimpleVM
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Tipo = m.Tipo
                    })
                    .ToListAsync();
        }
    }
}
