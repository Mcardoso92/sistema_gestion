using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel.ProductoImportacion;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ProductoImportacionController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IProductoImportacionService _importacionService;

        public ProductoImportacionController(SaasDbContext context, UserManager<Usuario> userManager, IProductoImportacionService importacionService)
        {
            _context = context;
            _userManager = userManager;
            _importacionService = importacionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            await CargarEmpresasAsync(usuario);
            return View(new ProductoImportacionArchivoVM { EmpresaId = usuario.EmpresaId });
        }

        [HttpGet]
        public IActionResult DescargarPlantilla()
        {
            byte[] archivo = _importacionService.GenerarPlantilla();
            return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PlantillaProductosVeltika.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analizar(ProductoImportacionArchivoVM modelo)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaId = await ResolverEmpresaAsync(usuario, modelo.EmpresaId);
            if (!empresaId.HasValue) ModelState.AddModelError(nameof(modelo.EmpresaId), "La empresa seleccionada no es válida.");

            if (!ModelState.IsValid)
            {
                await CargarEmpresasAsync(usuario);
                return View(nameof(Index), modelo);
            }

            try
            {
                ProductoImportacionVistaPreviaVM vistaPrevia = await _importacionService.AnalizarAsync(modelo.Archivo!, empresaId!.Value, usuario.Id);
                return RedirectToAction(nameof(VistaPrevia), new { token = vistaPrevia.Token, empresaId = vistaPrevia.EmpresaId });
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError(nameof(modelo.Archivo), ex.Message);
            }
            catch
            {
                ModelState.AddModelError(nameof(modelo.Archivo), "No se pudo leer el archivo. Verifique que sea una plantilla Excel válida.");
            }

            await CargarEmpresasAsync(usuario);
            return View(nameof(Index), modelo);
        }

        [HttpGet]
        public async Task<IActionResult> VistaPrevia(string token, int empresaId)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            int? empresaPermitida = await ResolverEmpresaAsync(usuario, empresaId);
            if (!empresaPermitida.HasValue) return NotFound();

            if (!_importacionService.TryObtenerVistaPrevia(token, empresaPermitida.Value, usuario.Id, out ProductoImportacionVistaPreviaVM? vistaPrevia))
            {
                TempData["Error"] = "La vista previa venció. Vuelva a seleccionar el archivo.";
                return RedirectToAction(nameof(Index));
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
                int cantidad = await _importacionService.ImportarAsync(token, empresaPermitida.Value, usuario.Id);
                TempData["Success"] = $"Se importaron {cantidad} productos correctamente.";
                return RedirectToAction("Index", "Producto");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error y no se importó ningún producto.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<int?> ResolverEmpresaAsync(Usuario usuario, int empresaSolicitada)
        {
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            int empresaId = esSuperAdmin ? empresaSolicitada : usuario.EmpresaId;
            bool empresaValida = await _context.Empresas.AsNoTracking().AnyAsync(e => e.Id == empresaId && e.Estado);
            return empresaValida ? empresaId : null;
        }

        private async Task CargarEmpresasAsync(Usuario usuario)
        {
            if (await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                ViewData["Empresas"] = new SelectList(await _context.Empresas.AsNoTracking().Where(e => e.Estado).OrderBy(e => e.Nombre).ToListAsync(), "Id", "Nombre");
            }
        }
    }
}
