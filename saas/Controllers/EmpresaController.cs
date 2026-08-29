using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.Helpers;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class EmpresaController : Controller
    {
        private readonly SaasDbContext _context;

        public EmpresaController(SaasDbContext context)
        {
            _context = context;
        }

        // GET: Empresa
        public async Task<IActionResult> Index(string estado = "activos", string? busqueda = null)
        {
            IQueryable<Empresa> empresas = _context.Empresas
                .AsNoTracking();

            switch (estado.ToLower())
            {
                case "inactivos":
                    empresas = empresas.Where(e => !e.Estado);
                    break;

                case "todos":
                    break;

                default:
                    empresas = empresas.Where(e => e.Estado);
                    estado = "activos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                empresas = empresas.Where(e => e.Nombre.Contains(busqueda));
            }

            ViewBag.Estado = estado;
            ViewBag.Busqueda = busqueda;

            var listaEmpresas = await empresas
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(listaEmpresas);
        }

        // GET: Empresa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            return View(empresa);
        }

        // GET: Empresa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empresa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Empresa empresa)
        {
            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            bool existeEmpresa = await _context.Empresas.AnyAsync(e =>
                e.Nombre.ToLower() == empresa.Nombre.ToLower());

            if (existeEmpresa)
            {
                ModelState.AddModelError(
                    nameof(empresa.Nombre),
                    "Ya existe una empresa con ese nombre.");

                return View(empresa);
            }

            try
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var fechaAlta = DateTime.Now;

                    empresa.FechaAlta = fechaAlta;
                    empresa.Estado = true;

                    _context.Empresas.Add(empresa);

                    await _context.SaveChangesAsync();

                    var mediosPagoPredeterminados =
                        ConfiguracionInicialEmpresa
                            .CrearMediosPagoPredeterminados(
                                empresa.Id,
                                fechaAlta);

                    _context.MediosPago.AddRange(
                        mediosPagoPredeterminados);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Success"] =
                        "Empresa creada correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al crear la empresa.");

                return View(empresa);
            }
        }
        // GET: Empresa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }
            return View(empresa);
        }
        // POST: Empresa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Estado")] Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            var empresaDb = await _context.Empresas.FindAsync(id);
            if (empresaDb == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            bool existeEmpresa = await _context.Empresas.AnyAsync(e =>
                e.Id != empresa.Id &&
                e.Nombre.ToLower() == empresa.Nombre.ToLower());

            if (existeEmpresa)
            {
                ModelState.AddModelError(
                    nameof(empresa.Nombre),
                    "Ya existe una empresa con ese nombre.");

                return View(empresa);
            }

            try
            {
                empresaDb.Nombre = empresa.Nombre;
                empresaDb.Estado = empresa.Estado;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Empresa modificada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al modificar la empresa.");
                return View(empresa);
            }
        }
        // GET: Empresa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empresa == null)
            {
                return NotFound();
            }

            return View(empresa);
        }
        // POST: Empresa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var empresa = await _context.Empresas.FindAsync(id);

                if (empresa == null)
                {
                    return NotFound();
                }

                empresa.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Empresa desactivada correctamente.";
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al desactivar la empresa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
