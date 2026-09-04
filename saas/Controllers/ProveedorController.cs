using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class ProveedorController : VeltikaController
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ProveedorController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index(ProveedorIndexVM proveedorVM, int pagina = 1)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Proveedor> consulta = _context.Proveedores
                .AsNoTracking()
                .Include(p => p.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
                proveedorVM.EmpresaId = null;
            }
            else if (proveedorVM.EmpresaId.HasValue)
            {
                consulta = consulta.Where(p => p.EmpresaId == proveedorVM.EmpresaId.Value);
            }

            switch (proveedorVM.Estado?.ToLower())
            {
                case "inactivos":
                    consulta = consulta.Where(p => !p.Estado);
                    break;

                case "todos":
                    break;

                default:
                    consulta = consulta.Where(p => p.Estado);
                    proveedorVM.Estado = "activos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(proveedorVM.Busqueda))
            {
                string busqueda = proveedorVM.Busqueda.Trim();
                string cuitBusqueda =
                    CuitValidator.Normalizar(busqueda) ?? string.Empty;

                consulta = consulta.Where(p =>
                    p.RazonSocial.Contains(busqueda) ||
                    (p.NombreFantasia != null && p.NombreFantasia.Contains(busqueda)) ||
                    (p.Email != null && p.Email.Contains(busqueda)) ||
                    (!string.IsNullOrEmpty(cuitBusqueda) &&
                     p.CUIT != null &&
                     p.CUIT.Contains(cuitBusqueda)));
            }

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalProveedores = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalProveedores / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalProveedores;

            proveedorVM.Proveedores = await consulta
                .OrderBy(p => p.RazonSocial)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .Select(p => new ProveedorIndexItemVM
                {
                    Id = p.Id,
                    RazonSocial = p.RazonSocial,
                    NombreFantasia = p.NombreFantasia,
                    CUIT = p.CUIT,
                    Email = p.Email,
                    Telefono = p.Telefono,
                    EmpresaNombre = p.Empresa.Nombre,
                    Estado = p.Estado
                })
                .ToListAsync();

            foreach (ProveedorIndexItemVM proveedor in proveedorVM.Proveedores)
            {
                proveedor.CUIT = CuitValidator.Formatear(proveedor.CUIT);
            }

            if (esSuperAdmin)
            {
                proveedorVM.Empresas = await _context.Empresas
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

            return View(proveedorVM);
        }
        // GET: Proveedor/Details/5
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

            IQueryable<Proveedor> consulta = _context.Proveedores
                .AsNoTracking()
                .Include(p => p.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var proveedor = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            var proveedorVM = new ProveedorDetailsVM
            {
                Id = proveedor.Id,
                RazonSocial = proveedor.RazonSocial,
                NombreFantasia = proveedor.NombreFantasia,
                CUIT = CuitValidator.Formatear(proveedor.CUIT),
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Localidad = proveedor.Localidad,
                Provincia = proveedor.Provincia,
                CodigoPostal = proveedor.CodigoPostal,
                Observaciones = proveedor.Observaciones,
                Estado = proveedor.Estado,
                FechaAlta = proveedor.FechaAlta,
                EmpresaNombre = proveedor.Empresa.Nombre
            };

            return View(proveedorVM);
        }
        // GET: Proveedor/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            var proveedorVM = new ProveedorCreateVM();

            if (esSuperAdmin)
            {
                await CargarEmpresas(proveedorVM);
            }
            else
            {
                proveedorVM.EmpresaId = usuario.EmpresaId;
            }

            return View(proveedorVM);
        }
        // POST: Proveedor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorCreateVM proveedorVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                proveedorVM.EmpresaId = usuario.EmpresaId;
                ModelState.Remove(nameof(proveedorVM.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    await CargarEmpresas(proveedorVM);
                }

                return View(proveedorVM);
            }

            if (!proveedorVM.EmpresaId.HasValue)
            {
                ModelState.AddModelError(nameof(proveedorVM.EmpresaId), "Debe seleccionar una empresa.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(proveedorVM);
                }

                return View(proveedorVM);
            }

            bool empresaValida = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Id == proveedorVM.EmpresaId.Value &&
                    e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(nameof(proveedorVM.EmpresaId), "La empresa seleccionada no es válida.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(proveedorVM);
                }

                return View(proveedorVM);
            }

            proveedorVM.RazonSocial = proveedorVM.RazonSocial.Trim();
            proveedorVM.NombreFantasia = NormalizarTextoOpcional(proveedorVM.NombreFantasia);
            proveedorVM.Email = NormalizarTextoOpcional(proveedorVM.Email);
            proveedorVM.Telefono = NormalizarTextoOpcional(proveedorVM.Telefono);
            proveedorVM.Direccion = NormalizarTextoOpcional(proveedorVM.Direccion);
            proveedorVM.Localidad = NormalizarTextoOpcional(proveedorVM.Localidad);
            proveedorVM.Provincia = NormalizarTextoOpcional(proveedorVM.Provincia);
            proveedorVM.CodigoPostal = NormalizarTextoOpcional(proveedorVM.CodigoPostal);
            proveedorVM.Observaciones = NormalizarTextoOpcional(proveedorVM.Observaciones);

            string? cuitNormalizado = null;

            if (!string.IsNullOrWhiteSpace(proveedorVM.CUIT))
            {
                cuitNormalizado = CuitValidator.Normalizar(proveedorVM.CUIT)!;

                if (!CuitValidator.EsValido(cuitNormalizado))
                {
                    ModelState.AddModelError(nameof(proveedorVM.CUIT), "El CUIT ingresado no es válido.");

                    if (esSuperAdmin)
                    {
                        await CargarEmpresas(proveedorVM);
                    }

                    return View(proveedorVM);
                }

                bool existeCuit = await _context.Proveedores
                    .AsNoTracking()
                    .AnyAsync(p =>
                        p.EmpresaId == proveedorVM.EmpresaId.Value &&
                        p.CUIT == cuitNormalizado &&
                        p.Estado);

                if (existeCuit)
                {
                    ModelState.AddModelError(
                        nameof(proveedorVM.CUIT),
                        "Ya existe un proveedor activo con ese CUIT para esta empresa.");

                    if (esSuperAdmin)
                    {
                        await CargarEmpresas(proveedorVM);
                    }

                    return View(proveedorVM);
                }
            }

            var proveedor = new Proveedor
            {
                RazonSocial = proveedorVM.RazonSocial,
                NombreFantasia = proveedorVM.NombreFantasia,
                CUIT = cuitNormalizado,
                Email = proveedorVM.Email,
                Telefono = proveedorVM.Telefono,
                Direccion = proveedorVM.Direccion,
                Localidad = proveedorVM.Localidad,
                Provincia = proveedorVM.Provincia,
                CodigoPostal = proveedorVM.CodigoPostal,
                Observaciones = proveedorVM.Observaciones,
                Estado = true,
                FechaAlta = DateTime.Now,
                EmpresaId = proveedorVM.EmpresaId.Value
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor creado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // GET: Proveedor/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
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

            IQueryable<Proveedor> consulta = _context.Proveedores
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var proveedor = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            var proveedorVM = new ProveedorEditVM
            {
                Id = proveedor.Id,
                RazonSocial = proveedor.RazonSocial,
                NombreFantasia = proveedor.NombreFantasia,
                CUIT = CuitValidator.Formatear(proveedor.CUIT),
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Localidad = proveedor.Localidad,
                Provincia = proveedor.Provincia,
                CodigoPostal = proveedor.CodigoPostal,
                Observaciones = proveedor.Observaciones,
                Estado = proveedor.Estado
            };

            return View(proveedorVM);
        }
        // POST: Proveedor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProveedorEditVM proveedorVM)
        {
            if (id != proveedorVM.Id)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Proveedor> consulta = _context.Proveedores;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var proveedor = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(proveedorVM);
            }

            proveedorVM.RazonSocial = proveedorVM.RazonSocial.Trim();
            proveedorVM.NombreFantasia = NormalizarTextoOpcional(proveedorVM.NombreFantasia);
            proveedorVM.Email = NormalizarTextoOpcional(proveedorVM.Email);
            proveedorVM.Telefono = NormalizarTextoOpcional(proveedorVM.Telefono);
            proveedorVM.Direccion = NormalizarTextoOpcional(proveedorVM.Direccion);
            proveedorVM.Localidad = NormalizarTextoOpcional(proveedorVM.Localidad);
            proveedorVM.Provincia = NormalizarTextoOpcional(proveedorVM.Provincia);
            proveedorVM.CodigoPostal = NormalizarTextoOpcional(proveedorVM.CodigoPostal);
            proveedorVM.Observaciones = NormalizarTextoOpcional(proveedorVM.Observaciones);

            string? cuitNormalizado = null;

            if (!string.IsNullOrWhiteSpace(proveedorVM.CUIT))
            {
                cuitNormalizado = CuitValidator.Normalizar(proveedorVM.CUIT)!;

                if (!CuitValidator.EsValido(cuitNormalizado))
                {
                    ModelState.AddModelError(nameof(proveedorVM.CUIT), "El CUIT ingresado no es válido.");
                    return View(proveedorVM);
                }

                bool existeCuit = await _context.Proveedores
                    .AsNoTracking()
                    .AnyAsync(p =>
                        p.Id != proveedor.Id &&
                        p.EmpresaId == proveedor.EmpresaId &&
                        p.CUIT == cuitNormalizado &&
                        p.Estado);

                if (existeCuit)
                {
                    ModelState.AddModelError(
                        nameof(proveedorVM.CUIT),
                        "Ya existe un proveedor activo con ese CUIT para esta empresa.");

                    return View(proveedorVM);
                }
            }

            proveedor.RazonSocial = proveedorVM.RazonSocial;
            proveedor.NombreFantasia = proveedorVM.NombreFantasia;
            proveedor.CUIT = cuitNormalizado;
            proveedor.Email = proveedorVM.Email;
            proveedor.Telefono = proveedorVM.Telefono;
            proveedor.Direccion = proveedorVM.Direccion;
            proveedor.Localidad = proveedorVM.Localidad;
            proveedor.Provincia = proveedorVM.Provincia;
            proveedor.CodigoPostal = proveedorVM.CodigoPostal;
            proveedor.Observaciones = proveedorVM.Observaciones;
            proveedor.Estado = proveedorVM.Estado;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        // GET: Proveedor/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
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

            IQueryable<Proveedor> consulta = _context.Proveedores
                .AsNoTracking()
                .Include(p => p.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var proveedor = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            if (!proveedor.Estado)
            {
                TempData["Error"] = "El proveedor ya se encuentra inactivo.";
                return RedirectToAction(nameof(Index));
            }

            var proveedorVM = new ProveedorDetailsVM
            {
                Id = proveedor.Id,
                RazonSocial = proveedor.RazonSocial,
                NombreFantasia = proveedor.NombreFantasia,
                CUIT = CuitValidator.Formatear(proveedor.CUIT),
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Localidad = proveedor.Localidad,
                Provincia = proveedor.Provincia,
                CodigoPostal = proveedor.CodigoPostal,
                Observaciones = proveedor.Observaciones,
                Estado = proveedor.Estado,
                FechaAlta = proveedor.FechaAlta,
                EmpresaNombre = proveedor.Empresa.Nombre
            };

            return View(proveedorVM);
        }
        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Proveedor> consulta = _context.Proveedores;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            var proveedor = await consulta.FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            if (!proveedor.Estado)
            {
                TempData["Error"] = "El proveedor ya se encuentra inactivo.";
                return RedirectToAction(nameof(Index));
            }

            proveedor.Estado = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proveedor desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        private static string? NormalizarTextoOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
        private async Task CargarEmpresas(ProveedorCreateVM proveedorVM)
        {
            proveedorVM.Empresas = await _context.Empresas
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
}
