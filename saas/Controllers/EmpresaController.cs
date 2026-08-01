using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Empresas.ToListAsync());
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
        public async Task<IActionResult> Create(Empresa empresa)
        {
            if (!ModelState.IsValid)
            {
                return View(empresa);
            }
            try
            {
                empresa.FechaAlta = DateTime.Now;
                empresa.Estado = true;

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Empresa creada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar la empresa.");

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
        public async Task<IActionResult> Edit(int id, Empresa empresa)
        {
            var empresaDb = await _context.Empresas.FindAsync(id);
            if (empresaDb == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
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
                if (empresa != null)
                {
                    empresa.Estado = false;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Empresa desactivada correctamente.";
            }
            catch
            {
                TempData["Error"] = "No es posible eliminar la empresa porque tiene información relacionada.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
