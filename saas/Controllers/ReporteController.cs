using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel.Reportes;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ReporteController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ReporteController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Ventas(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? clienteId,
            int? empresaId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            ViewBag.EsSuperAdmin = esSuperAdmin;

            DateTime hoy = DateTime.Today;
            DateTime desde = fechaDesde?.Date
                ?? new DateTime(hoy.Year, hoy.Month, 1);
            DateTime hasta = fechaHasta?.Date
                ?? hoy;

            var vm = new ReporteVentasVM
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                ClienteId = clienteId,
                EmpresaId = esSuperAdmin
                    ? empresaId
                    : usuario.EmpresaId
            };

            if (hasta < desde)
            {
                ModelState.AddModelError(
                    "",
                    "La fecha hasta no puede ser anterior a la fecha desde.");

                await CargarOpciones(
                    vm,
                    esSuperAdmin,
                    usuario.EmpresaId);

                return View(vm);
            }

            if (esSuperAdmin && empresaId.HasValue)
            {
                bool empresaValida = await _context.Empresas
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.Id == empresaId.Value &&
                        e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(vm.EmpresaId),
                        "La empresa seleccionada no es válida.");

                    vm.EmpresaId = null;

                    await CargarOpciones(
                        vm,
                        esSuperAdmin,
                        usuario.EmpresaId);

                    return View(vm);
                }
            }

            DateTime hastaExclusivo = hasta.AddDays(1);

            IQueryable<Venta> consulta = _context.Ventas
                .AsNoTracking()
                .Where(v =>
                    v.Estado &&
                    v.Fecha >= desde &&
                    v.Fecha < hastaExclusivo);

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consulta = consulta.Where(v =>
                        v.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consulta = consulta.Where(v =>
                    v.EmpresaId == usuario.EmpresaId);
            }

            if (clienteId.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.ClienteId == clienteId.Value);
            }

            vm.Ventas = await consulta
                .OrderByDescending(v => v.Fecha)
                .ThenByDescending(v => v.Id)
                .Select(v => new ReporteVentaFilaVM
                {
                    VentaId = v.Id,
                    Fecha = v.Fecha,
                    Cliente = v.Cliente == null
                        ? "Consumidor final"
                        : v.Cliente.Nombre +
                            (v.Cliente.Apellido == null
                                ? ""
                                : " " + v.Cliente.Apellido),
                    Usuario = v.Usuario.Nombre + " " + v.Usuario.Apellido,
                    CantidadProductos = v.Detalles.Sum(d => d.Cantidad),
                    Total = v.Total
                })
                .ToListAsync();

            vm.TotalVendido = vm.Ventas.Sum(v => v.Total);
            vm.CantidadVentas = vm.Ventas.Count;
            vm.TicketPromedio = vm.CantidadVentas > 0
                ? vm.TotalVendido / vm.CantidadVentas
                : 0;

            await CargarOpciones(
                vm,
                esSuperAdmin,
                usuario.EmpresaId);

            

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Stock(
            int? categoriaId,
            int? empresaId,
            string situacion = "todos")
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            ViewBag.EsSuperAdmin = esSuperAdmin;

            string[] situacionesValidas =
            {
                "todos",
                "normal",
                "bajo",
                "sin-stock"
            };

            if (!situacionesValidas.Contains(situacion))
            {
                situacion = "todos";
            }

            var vm = new ReporteStockVM
            {
                CategoriaId = categoriaId,
                EmpresaId = esSuperAdmin
                    ? empresaId
                    : usuario.EmpresaId,
                Situacion = situacion
            };

            if (esSuperAdmin && empresaId.HasValue)
            {
                bool empresaValida = await _context.Empresas
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.Id == empresaId.Value &&
                        e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(vm.EmpresaId),
                        "La empresa seleccionada no es válida.");

                    vm.EmpresaId = null;

                    await CargarOpcionesStock(
                        vm,
                        esSuperAdmin,
                        usuario.EmpresaId);

                    return View(vm);
                }
            }

            IQueryable<Producto> consulta = _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado);

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consulta = consulta.Where(p =>
                        p.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consulta = consulta.Where(p =>
                    p.EmpresaId == usuario.EmpresaId);
            }

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }

            consulta = situacion switch
            {
                "normal" => consulta.Where(p =>
                    p.Stock > p.PuntoReposicion),

                "bajo" => consulta.Where(p =>
                    p.Stock > 0 &&
                    p.Stock <= p.PuntoReposicion),

                "sin-stock" => consulta.Where(p =>
                    p.Stock == 0),

                _ => consulta
            };

            vm.Productos = await consulta
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Nombre)
                .Select(p => new ReporteStockFilaVM
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarra = p.CodigoBarra,
                    Categoria = p.Categoria.Nombre,
                    Empresa = p.Empresa.Nombre,
                    Stock = p.Stock,
                    PuntoReposicion = p.PuntoReposicion,
                    PrecioCosto = p.PrecioCosto,
                    PrecioVenta = p.PrecioVenta,
                    ValorCosto = p.PrecioCosto * p.Stock,
                    ValorVenta = p.PrecioVenta * p.Stock,
                    Situacion = p.Stock == 0
                        ? "Sin stock"
                        : p.Stock <= p.PuntoReposicion
                            ? "Stock bajo"
                            : "Normal"
                })
                .ToListAsync();

            vm.CantidadProductos = vm.Productos.Count;
            vm.UnidadesStock = vm.Productos.Sum(p => p.Stock);
            vm.ProductosStockBajo = vm.Productos.Count(p =>
                p.Stock <= p.PuntoReposicion);
            vm.ValorInventarioCosto = vm.Productos.Sum(p =>
                p.ValorCosto);
            vm.ValorInventarioVenta = vm.Productos.Sum(p =>
                p.ValorVenta);

            await CargarOpcionesStock(
                vm,
                esSuperAdmin,
                usuario.EmpresaId);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Productos(
            int? categoriaId,
            int? empresaId,
            string estado = "activos",
            string? busqueda = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            ViewBag.EsSuperAdmin = esSuperAdmin;

            string[] estadosValidos =
            {
                "activos",
                "inactivos",
                "todos"
            };

            if (!estadosValidos.Contains(estado))
            {
                estado = "activos";
            }

            busqueda = string.IsNullOrWhiteSpace(busqueda)
                ? null
                : busqueda.Trim();

            var vm = new ReporteProductosVM
            {
                CategoriaId = categoriaId,
                EmpresaId = esSuperAdmin
                    ? empresaId
                    : usuario.EmpresaId,
                Estado = estado,
                Busqueda = busqueda
            };

            if (esSuperAdmin && empresaId.HasValue)
            {
                bool empresaValida = await _context.Empresas
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.Id == empresaId.Value &&
                        e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(vm.EmpresaId),
                        "La empresa seleccionada no es válida.");

                    vm.EmpresaId = null;

                    await CargarOpcionesProductos(
                        vm,
                        esSuperAdmin,
                        usuario.EmpresaId);

                    return View(vm);
                }
            }

            IQueryable<Producto> consulta = _context.Productos
                .AsNoTracking();

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consulta = consulta.Where(p =>
                        p.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consulta = consulta.Where(p =>
                    p.EmpresaId == usuario.EmpresaId);
            }

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }

            if (busqueda != null)
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    (p.CodigoBarra != null &&
                     p.CodigoBarra.Contains(busqueda)));
            }

            consulta = estado switch
            {
                "activos" => consulta.Where(p => p.Estado),
                "inactivos" => consulta.Where(p => !p.Estado),
                _ => consulta
            };

            vm.Productos = await consulta
                .OrderBy(p => p.Nombre)
                .Select(p => new ReporteProductoFilaVM
                {
                    ProductoId = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarra = p.CodigoBarra,
                    Categoria = p.Categoria.Nombre,
                    Empresa = p.Empresa.Nombre,
                    PrecioCosto = p.PrecioCosto,
                    PrecioVenta = p.PrecioVenta,
                    MargenImporte = p.PrecioVenta - p.PrecioCosto,
                    MargenPorcentaje = p.PrecioCosto > 0
                        ? (p.PrecioVenta - p.PrecioCosto) /
                            p.PrecioCosto * 100
                        : 0,
                    Stock = p.Stock,
                    Estado = p.Estado
                })
                .ToListAsync();

            vm.CantidadProductos = vm.Productos.Count;
            vm.ProductosActivos = vm.Productos.Count(p => p.Estado);
            vm.ProductosInactivos = vm.Productos.Count(p => !p.Estado);
            vm.MargenPromedioPorcentaje = vm.Productos.Any()
                ? vm.Productos.Average(p => p.MargenPorcentaje)
                : 0;

            await CargarOpcionesProductos(
                vm,
                esSuperAdmin,
                usuario.EmpresaId);

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Clientes(
            int? empresaId,
            string estado = "activos",
            string actividad = "todos",
            string? busqueda = null)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            ViewBag.EsSuperAdmin = esSuperAdmin;

            string[] estadosValidos =
            {
                "activos",
                "inactivos",
                "todos"
            };

            string[] actividadesValidas =
            {
                "todos",
                "con-compras",
                "sin-compras"
            };

            if (!estadosValidos.Contains(estado))
            {
                estado = "activos";
            }

            if (!actividadesValidas.Contains(actividad))
            {
                actividad = "todos";
            }

            busqueda = string.IsNullOrWhiteSpace(busqueda)
                ? null
                : busqueda.Trim();

            var vm = new ReporteClientesVM
            {
                EmpresaId = esSuperAdmin
                    ? empresaId
                    : usuario.EmpresaId,
                Estado = estado,
                Actividad = actividad,
                Busqueda = busqueda
            };

            if (esSuperAdmin && empresaId.HasValue)
            {
                bool empresaValida = await _context.Empresas
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.Id == empresaId.Value &&
                        e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(vm.EmpresaId),
                        "La empresa seleccionada no es válida.");

                    vm.EmpresaId = null;

                    await CargarOpcionesClientes(vm);

                    return View(vm);
                }
            }

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking();

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consulta = consulta.Where(c =>
                        c.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consulta = consulta.Where(c =>
                    c.EmpresaId == usuario.EmpresaId);
            }

            consulta = estado switch
            {
                "activos" => consulta.Where(c => c.Estado),
                "inactivos" => consulta.Where(c => !c.Estado),
                _ => consulta
            };

            consulta = actividad switch
            {
                "con-compras" => consulta.Where(c =>
                    c.Ventas.Any(v => v.Estado)),

                "sin-compras" => consulta.Where(c =>
                    !c.Ventas.Any(v => v.Estado)),

                _ => consulta
            };

            if (busqueda != null)
            {
                consulta = consulta.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    (c.Apellido != null &&
                     c.Apellido.Contains(busqueda)) ||
                    (c.Documento != null &&
                     c.Documento.Contains(busqueda)) ||
                    (c.Email != null &&
                     c.Email.Contains(busqueda)));
            }

            vm.Clientes = await consulta
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Select(c => new ReporteClienteFilaVM
                {
                    ClienteId = c.Id,
                    NombreCompleto = c.Nombre +
                        (c.Apellido == null
                            ? ""
                            : " " + c.Apellido),
                    Documento = c.Documento,
                    Email = c.Email,
                    Telefono = c.Telefono,
                    Empresa = c.Empresa.Nombre,
                    CantidadCompras = c.Ventas.Count(v => v.Estado),
                    ImporteComprado = c.Ventas
                        .Where(v => v.Estado)
                        .Sum(v => (decimal?)v.Total)
                        ?? 0,
                    UltimaCompra = c.Ventas
                        .Where(v => v.Estado)
                        .Select(v => (DateTime?)v.Fecha)
                        .Max(),
                    Estado = c.Estado
                })
                .ToListAsync();

            vm.CantidadClientes = vm.Clientes.Count;
            vm.ClientesActivos = vm.Clientes.Count(c => c.Estado);
            vm.ClientesInactivos = vm.Clientes.Count(c => !c.Estado);
            vm.ClientesConCompras = vm.Clientes.Count(c =>
                c.CantidadCompras > 0);
            vm.ImporteTotalComprado = vm.Clientes.Sum(c =>
                c.ImporteComprado);

            await CargarOpcionesClientes(vm);

            return View(vm);
        }

        private async Task CargarOpciones(
            ReporteVentasVM vm,
            bool esSuperAdmin,
            int empresaUsuarioId)
        {
            if (esSuperAdmin)
            {
                vm.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre,
                        Selected = vm.EmpresaId == e.Id
                    })
                    .ToListAsync();
            }

            IQueryable<Cliente> consultaClientes = _context.Clientes
                .AsNoTracking()
                .Where(c => c.Estado);

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consultaClientes = consultaClientes.Where(c =>
                        c.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consultaClientes = consultaClientes.Where(c =>
                    c.EmpresaId == empresaUsuarioId);
            }

            var clientes = await consultaClientes
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Apellido
                })
                .ToListAsync();

            vm.Clientes = clientes
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(c.Apellido)
                        ? c.Nombre
                        : $"{c.Nombre} {c.Apellido}",
                    Selected = vm.ClienteId == c.Id
                })
                .ToList();
        }

        private async Task CargarOpcionesStock(
            ReporteStockVM vm,
            bool esSuperAdmin,
            int empresaUsuarioId)
        {
            if (esSuperAdmin)
            {
                vm.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre,
                        Selected = vm.EmpresaId == e.Id
                    })
                    .ToListAsync();
            }

            IQueryable<Categoria> consultaCategorias = _context.Categorias
                .AsNoTracking()
                .Where(c => c.Estado);

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consultaCategorias = consultaCategorias.Where(c =>
                        c.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consultaCategorias = consultaCategorias.Where(c =>
                    c.EmpresaId == empresaUsuarioId);
            }

            vm.Categorias = await consultaCategorias
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre,
                    Selected = vm.CategoriaId == c.Id
                })
                .ToListAsync();
        }

        private async Task CargarOpcionesProductos(
            ReporteProductosVM vm,
            bool esSuperAdmin,
            int empresaUsuarioId)
        {
            if (esSuperAdmin)
            {
                vm.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre,
                        Selected = vm.EmpresaId == e.Id
                    })
                    .ToListAsync();
            }

            IQueryable<Categoria> consultaCategorias = _context.Categorias
                .AsNoTracking()
                .Where(c => c.Estado);

            if (esSuperAdmin)
            {
                if (vm.EmpresaId.HasValue)
                {
                    consultaCategorias = consultaCategorias.Where(c =>
                        c.EmpresaId == vm.EmpresaId.Value);
                }
            }
            else
            {
                consultaCategorias = consultaCategorias.Where(c =>
                    c.EmpresaId == empresaUsuarioId);
            }

            vm.Categorias = await consultaCategorias
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre,
                    Selected = vm.CategoriaId == c.Id
                })
                .ToListAsync();
        }

        private async Task CargarOpcionesClientes(
            ReporteClientesVM vm)
        {
            vm.Empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre,
                    Selected = vm.EmpresaId == e.Id
                })
                .ToListAsync();
        }
    }
}