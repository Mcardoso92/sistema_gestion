using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel.Configuracion;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ConfiguracionController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ConfiguracionController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? empresaId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            ViewBag.EsSuperAdmin = esSuperAdmin;

            int? empresaPermitidaId = esSuperAdmin ? empresaId : usuario.EmpresaId;

            if (esSuperAdmin && !empresaPermitidaId.HasValue)
            {
                empresaPermitidaId = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => (int?)e.Id)
                    .FirstOrDefaultAsync();
            }

            var vm = new ConfiguracionEmpresaVM
            {
                EmpresaId = empresaPermitidaId
            };

            if (!empresaPermitidaId.HasValue)
            {
                ModelState.AddModelError("", "No hay una empresa disponible para configurar.");
                await CargarOpciones(vm, esSuperAdmin);
                return View(vm);
            }

            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == empresaPermitidaId.Value && e.Estado);

            if (empresa == null)
            {
                return NotFound();
            }

            var configuracion = await _context.ConfiguracionesEmpresa
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.EmpresaId == empresa.Id);

            vm.EmpresaNombre = empresa.Nombre;
            vm.RazonSocial = configuracion?.RazonSocial ?? empresa.Nombre;
            vm.Cuit = configuracion?.Cuit;
            vm.Direccion = configuracion?.Direccion;
            vm.Telefono = configuracion?.Telefono;
            vm.Email = configuracion?.Email;
            vm.Moneda = configuracion?.Moneda ?? "ARS";
            vm.IvaPorcentaje = configuracion?.IvaPorcentaje ?? 21;
            vm.MontoVentaImportante = configuracion?.MontoVentaImportante;
            vm.LogoRuta = configuracion?.LogoRuta;

            await CargarOpciones(vm, esSuperAdmin);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConfiguracionEmpresaVM vm)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            ViewBag.EsSuperAdmin = esSuperAdmin;
            vm.EmpresaId = esSuperAdmin ? vm.EmpresaId : usuario.EmpresaId;

            string[] monedasValidas = { "ARS", "USD", "EUR" };
            vm.Moneda = vm.Moneda.Trim().ToUpperInvariant();

            if (!monedasValidas.Contains(vm.Moneda))
            {
                ModelState.AddModelError(nameof(vm.Moneda), "La moneda seleccionada no es válida.");
            }

            if (!vm.EmpresaId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.EmpresaId), "Debe seleccionar una empresa.");
            }

            Empresa? empresa = null;

            if (vm.EmpresaId.HasValue)
            {
                empresa = await _context.Empresas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == vm.EmpresaId.Value && e.Estado);

                if (empresa == null)
                {
                    ModelState.AddModelError(nameof(vm.EmpresaId), "La empresa seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.EmpresaNombre = empresa?.Nombre ?? string.Empty;
                await CargarOpciones(vm, esSuperAdmin);
                return View(vm);
            }

            var configuracion = await _context.ConfiguracionesEmpresa
                .FirstOrDefaultAsync(c => c.EmpresaId == vm.EmpresaId!.Value);

            if (configuracion == null)
            {
                configuracion = new ConfiguracionEmpresa
                {
                    EmpresaId = vm.EmpresaId.GetValueOrDefault()
                };

                _context.ConfiguracionesEmpresa.Add(configuracion);
            }

            configuracion.RazonSocial = vm.RazonSocial.Trim();
            configuracion.Cuit = LimpiarTexto(vm.Cuit);
            configuracion.Direccion = LimpiarTexto(vm.Direccion);
            configuracion.Telefono = LimpiarTexto(vm.Telefono);
            configuracion.Email = LimpiarTexto(vm.Email);
            configuracion.Moneda = vm.Moneda;
            configuracion.IvaPorcentaje = vm.IvaPorcentaje;
            configuracion.MontoVentaImportante = vm.MontoVentaImportante;

            await _context.SaveChangesAsync();

            TempData["Success"] = "La configuración se guardó correctamente.";

            return RedirectToAction(nameof(Index), new { empresaId = vm.EmpresaId });
        }

        private async Task CargarOpciones(ConfiguracionEmpresaVM vm, bool esSuperAdmin)
        {
            vm.Monedas = new List<SelectListItem>
            {
                new SelectListItem("Peso argentino (ARS)", "ARS"),
                new SelectListItem("Dólar estadounidense (USD)", "USD"),
                new SelectListItem("Euro (EUR)", "EUR")
            };

            if (esSuperAdmin)
            {
                vm.Empresas = await _context.Empresas
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
        }

        private static string? LimpiarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}