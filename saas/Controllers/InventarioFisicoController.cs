using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel.InventarioFisico;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class InventarioFisicoController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly InventarioFisicoService _service;
        private readonly IFechaHoraService _fechaHora;

        public InventarioFisicoController(SaasDbContext context, UserManager<Usuario> userManager, InventarioFisicoService service, IFechaHoraService fechaHora)
        {
            _context = context;
            _userManager = userManager;
            _service = service;
            _fechaHora = fechaHora;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? empresaId = null)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            IQueryable<InventarioFisico> consulta = _context.InventariosFisicos.AsNoTracking()
                .Include(i => i.Empresa).Include(i => i.UsuarioInicio).Include(i => i.Detalles);

            if (!esSuperAdmin)
            {
                empresaId = usuario.EmpresaId;
                consulta = consulta.Where(i => i.EmpresaId == usuario.EmpresaId);
            }
            else if (empresaId.HasValue)
            {
                consulta = consulta.Where(i => i.EmpresaId == empresaId.Value);
            }

            ViewBag.EmpresaId = empresaId;
            ViewBag.Empresas = esSuperAdmin
                ? new SelectList(await _context.Empresas.AsNoTracking().Where(e => e.Estado).OrderBy(e => e.Nombre).ToListAsync(), "Id", "Nombre", empresaId)
                : null;

            return View(await consulta.OrderByDescending(i => i.FechaInicio).ThenByDescending(i => i.Id).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? empresaId = null, int? categoriaId = null, string? busqueda = null, int pagina = 1)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaInicialAsync(usuario, empresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            var modelo = new InventarioFisicoCrearVM { EmpresaId = empresaPermitida.Value, CategoriaId = categoriaId, Busqueda = busqueda, PaginaActual = pagina };
            await CargarCrearAsync(modelo, usuario);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preparar(InventarioFisicoCrearVM modelo, int pagina = 1)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaAsync(usuario, modelo.EmpresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            modelo.EmpresaId = empresaPermitida.Value;
            modelo.PaginaActual = pagina;
            await CargarCrearAsync(modelo, usuario);

            // Filtrar o paginar no intenta crear la sesión, por eso no mostramos
            // validaciones de campos que el usuario todavía puede estar completando.
            ModelState.Clear();
            return View(nameof(Create), modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventarioFisicoCrearVM modelo)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaAsync(usuario, modelo.EmpresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            if (modelo.ProductosIds.Count == 0)
            {
                ModelState.AddModelError(nameof(modelo.ProductosIds), "Seleccione al menos un producto.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCrearAsync(modelo, usuario);
                return View(modelo);
            }

            try
            {
                InventarioFisico inventario = await _service.CrearAsync(empresaPermitida.Value, usuario.Id, modelo.Motivo, modelo.ProductosIds);
                TempData["Success"] = "Inventario físico iniciado correctamente. El stock todavía no fue modificado.";
                return RedirectToAction(nameof(Details), new { id = inventario.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarCrearAsync(modelo, usuario);
                return View(modelo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            InventarioFisico? inventario = await ObtenerPermitidoAsync(id, usuario);
            return inventario == null ? NotFound() : View(CrearDetailsVM(inventario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConteo(InventarioFisicoDetailsVM modelo)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            InventarioFisico? inventario = await ObtenerPermitidoAsync(modelo.Id, usuario);
            if (inventario == null) return NotFound();

            try
            {
                await _service.GuardarConteoAsync(inventario.Id, inventario.EmpresaId, modelo.Detalles.ToDictionary(d => d.DetalleId, d => d.StockContado));
                TempData["Success"] = "Conteo guardado. El stock todavía no fue modificado.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = modelo.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(InventarioFisicoDetailsVM modelo)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            InventarioFisico? inventario = await ObtenerPermitidoAsync(modelo.Id, usuario);
            if (inventario == null) return NotFound();

            try
            {
                await _service.GuardarConteoAsync(inventario.Id, inventario.EmpresaId, modelo.Detalles.ToDictionary(d => d.DetalleId, d => d.StockContado));
                int ajustes = await _service.ConfirmarAsync(inventario.Id, inventario.EmpresaId, usuario.Id);
                TempData["Success"] = $"Inventario confirmado correctamente. Se generaron {ajustes} movimientos de ajuste.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = modelo.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();
            InventarioFisico? inventario = await ObtenerPermitidoAsync(id, usuario);
            if (inventario == null) return NotFound();

            try
            {
                await _service.CancelarAsync(inventario.Id, inventario.EmpresaId);
                TempData["Success"] = "Inventario cancelado. No se modificó el stock.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task CargarCrearAsync(InventarioFisicoCrearVM modelo, Usuario usuario)
        {
            if (await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                ViewData["Empresas"] = new SelectList(await _context.Empresas.AsNoTracking().Where(e => e.Estado).OrderBy(e => e.Nombre).ToListAsync(), "Id", "Nombre", modelo.EmpresaId);
            }

            ViewData["Categorias"] = new SelectList(await _context.Categorias.AsNoTracking().Where(c => c.EmpresaId == modelo.EmpresaId && c.Estado).OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", modelo.CategoriaId);
            IQueryable<Producto> productosEmpresa = _context.Productos.AsNoTracking()
                .Where(p => p.EmpresaId == modelo.EmpresaId && p.Estado && p.ControlaStock);

            // Descarta selecciones que no pertenezcan a la empresa elegida, pero
            // conserva las realizadas en otras páginas del mismo inventario.
            if (modelo.ProductosIds.Count > 0)
            {
                modelo.ProductosIds = await productosEmpresa
                    .Where(p => modelo.ProductosIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .Distinct()
                    .ToListAsync();
            }

            IQueryable<Producto> productos = productosEmpresa.Include(p => p.Categoria);

            if (modelo.CategoriaId.HasValue) productos = productos.Where(p => p.CategoriaId == modelo.CategoriaId.Value);
            if (!string.IsNullOrWhiteSpace(modelo.Busqueda))
            {
                string busqueda = modelo.Busqueda.Trim();
                productos = productos.Where(p => p.Nombre.Contains(busqueda) || (p.CodigoBarra != null && p.CodigoBarra.Contains(busqueda)));
            }

            const int tamanioPagina = 20;
            modelo.TotalProductos = await productos.CountAsync();
            modelo.TotalPaginas = (int)Math.Ceiling(modelo.TotalProductos / (double)tamanioPagina);
            modelo.PaginaActual = Math.Max(1, modelo.PaginaActual);
            if (modelo.TotalPaginas > 0 && modelo.PaginaActual > modelo.TotalPaginas)
            {
                modelo.PaginaActual = modelo.TotalPaginas;
            }

            modelo.Productos = await productos.OrderBy(p => p.Nombre)
                .Skip((modelo.PaginaActual - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new InventarioFisicoProductoSeleccionVM
            {
                Id = p.Id, Nombre = p.Nombre, CodigoBarra = p.CodigoBarra, Categoria = p.Categoria.Nombre, Stock = p.Stock
            }).ToListAsync();
        }

        private async Task<InventarioFisico?> ObtenerPermitidoAsync(int id, Usuario usuario)
        {
            IQueryable<InventarioFisico> consulta = _context.InventariosFisicos.AsNoTracking()
                .Include(i => i.Empresa).Include(i => i.UsuarioInicio).Include(i => i.UsuarioConfirmacion)
                .Include(i => i.Detalles).ThenInclude(d => d.Producto).ThenInclude(p => p.Categoria);
            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin")) consulta = consulta.Where(i => i.EmpresaId == usuario.EmpresaId);
            return await consulta.FirstOrDefaultAsync(i => i.Id == id);
        }

        private InventarioFisicoDetailsVM CrearDetailsVM(InventarioFisico inventario) => new()
        {
            Id = inventario.Id, EmpresaId = inventario.EmpresaId, Empresa = inventario.Empresa.Nombre,
            Motivo = inventario.Motivo, Estado = inventario.Estado,
            FechaInicio = _fechaHora.ConvertirAHoraLocal(inventario.FechaInicio),
            FechaConfirmacion = _fechaHora.ConvertirAHoraLocal(inventario.FechaConfirmacion),
            UsuarioInicio = $"{inventario.UsuarioInicio.Nombre} {inventario.UsuarioInicio.Apellido}",
            UsuarioConfirmacion = inventario.UsuarioConfirmacion == null ? null : $"{inventario.UsuarioConfirmacion.Nombre} {inventario.UsuarioConfirmacion.Apellido}",
            Detalles = inventario.Detalles.OrderBy(d => d.Producto.Nombre).Select(d => new InventarioFisicoDetalleVM
            {
                DetalleId = d.Id, ProductoId = d.ProductoId, Producto = d.Producto.Nombre,
                CodigoBarra = d.Producto.CodigoBarra, Categoria = d.Producto.Categoria.Nombre,
                StockTeorico = d.StockTeorico, StockContado = d.StockContado
            }).ToList()
        };

        private async Task<int?> ResolverEmpresaInicialAsync(Usuario usuario, int? empresaSolicitada)
        {
            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin")) return usuario.EmpresaId;
            if (empresaSolicitada.HasValue && await _context.Empresas.AsNoTracking().AnyAsync(e => e.Id == empresaSolicitada && e.Estado)) return empresaSolicitada;
            return await _context.Empresas.AsNoTracking().Where(e => e.Estado).OrderBy(e => e.Nombre).Select(e => (int?)e.Id).FirstOrDefaultAsync();
        }

        private async Task<int?> ResolverEmpresaAsync(Usuario usuario, int empresaSolicitada)
        {
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            int empresaId = esSuperAdmin ? empresaSolicitada : usuario.EmpresaId;
            return await _context.Empresas.AsNoTracking().AnyAsync(e => e.Id == empresaId && e.Estado) ? empresaId : null;
        }
    }
}
