using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Services;
using saas.ViewModel.ProductoActualizacionMasiva;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ProductoActualizacionMasivaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IProductoActualizacionMasivaService _service;

        public ProductoActualizacionMasivaController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            IProductoActualizacionMasivaService service)
        {
            _context = context;
            _userManager = userManager;
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? empresaId = null)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaSeleccionada = await ResolverEmpresaInicialAsync(usuario, empresaId);
            await CargarCombosAsync(usuario, empresaSeleccionada);

            return View(new ProductoActualizacionMasivaVM
            {
                EmpresaId = empresaSeleccionada ?? 0,
                Campo = CampoAjustePrecioProducto.PrecioVenta,
                TipoAjuste = TipoAjustePrecioProducto.Porcentaje
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Previsualizar(ProductoActualizacionMasivaVM modelo)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaId = await ResolverEmpresaAsync(usuario, modelo.EmpresaId);
            if (!empresaId.HasValue)
            {
                ModelState.AddModelError(nameof(modelo.EmpresaId), "La empresa seleccionada no es válida.");
            }

            if (!Enum.IsDefined(modelo.Campo))
            {
                ModelState.AddModelError(nameof(modelo.Campo), "Seleccione un precio válido.");
            }

            if (!Enum.IsDefined(modelo.TipoAjuste))
            {
                ModelState.AddModelError(nameof(modelo.TipoAjuste), "Seleccione un tipo de ajuste válido.");
            }

            if (modelo.ValorAjuste == 0)
            {
                ModelState.AddModelError(nameof(modelo.ValorAjuste), "El ajuste debe ser distinto de cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(usuario, empresaId ?? modelo.EmpresaId);
                return View(nameof(Index), modelo);
            }

            try
            {
                ProductoActualizacionMasivaVistaPreviaVM vistaPrevia =
                    await _service.PrepararVistaPreviaAsync(modelo, empresaId!.Value, usuario.Id);

                return RedirectToAction(
                    nameof(VistaPrevia),
                    new { token = vistaPrevia.Token, empresaId = vistaPrevia.EmpresaId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarCombosAsync(usuario, empresaId);
                return View(nameof(Index), modelo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VistaPrevia(string token, int empresaId)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaAsync(usuario, empresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            if (!_service.TryObtenerVistaPrevia(
                    token,
                    empresaPermitida.Value,
                    usuario.Id,
                    out ProductoActualizacionMasivaVistaPreviaVM? vistaPrevia))
            {
                TempData["Error"] = "La vista previa venció. Configure nuevamente la actualización.";
                return RedirectToAction(nameof(Index), new { empresaId });
            }

            return View(vistaPrevia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(string token, int empresaId)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaAsync(usuario, empresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            try
            {
                int cantidad = await _service.AplicarAsync(token, empresaPermitida.Value, usuario.Id);
                TempData["Success"] = $"Se actualizaron {cantidad} productos correctamente.";
                return RedirectToAction("Index", "Producto", new { empresaId = empresaPermitida.Value });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index), new { empresaId });
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error y no se modificó ningún producto.";
                return RedirectToAction(nameof(Index), new { empresaId });
            }
        }

        private async Task<int?> ResolverEmpresaInicialAsync(Usuario usuario, int? empresaSolicitada)
        {
            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                return usuario.EmpresaId;
            }

            if (empresaSolicitada.HasValue &&
                await _context.Empresas.AsNoTracking().AnyAsync(e => e.Id == empresaSolicitada && e.Estado))
            {
                return empresaSolicitada;
            }

            return await _context.Empresas.AsNoTracking()
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> ResolverEmpresaAsync(Usuario usuario, int empresaSolicitada)
        {
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            int empresaId = esSuperAdmin ? empresaSolicitada : usuario.EmpresaId;
            bool empresaValida = await _context.Empresas.AsNoTracking()
                .AnyAsync(e => e.Id == empresaId && e.Estado);
            return empresaValida ? empresaId : null;
        }

        private async Task CargarCombosAsync(Usuario usuario, int? empresaId)
        {
            if (await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                ViewData["Empresas"] = new SelectList(
                    await _context.Empresas.AsNoTracking()
                        .Where(e => e.Estado)
                        .OrderBy(e => e.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    empresaId);
            }

            ViewData["Categorias"] = new SelectList(
                await _context.Categorias.AsNoTracking()
                    .Where(c => empresaId.HasValue && c.EmpresaId == empresaId.Value && c.Estado)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");
        }
    }
}
