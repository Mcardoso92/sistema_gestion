using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

namespace saas.Controllers
{
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
                .FirstOrDefaultAsync(m => m.Id == id);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,ImagenEmpresa,Estado,FechaAlta")] Empresa empresa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    empresa.FechaAlta = DateTime.Now; // Carga por defecto la fecha de alta al momento de crear la empresa
                    empresa.Estado = true; // Carga por defecto el estado como activo al momento de crear la empresa

                    _context.Add(empresa);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Empresa creada correctamente.";

                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar la empresa.");

                return View(empresa);
            }

            // Si el modelo no es válido, regresar la vista con el modelo para mostrar errores
            return View(empresa);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,ImagenEmpresa,Estado,FechaAlta")] Empresa empresa)
        {
            var empresaDb = await _context.Empresas.FindAsync(id);
            if (empresaDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    empresaDb.Nombre = empresa.Nombre;
                    empresaDb.Estado = empresa.Estado;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpresaExists(empresa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["Success"] = "Empresa modificada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            return View(empresa);
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
                    _context.Empresas.Remove(empresa);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Empresa eliminada correctamente.";
            }
            catch
            {
                TempData["Error"] = "No es posible eliminar la empresa porque tiene información relacionada.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmpresaExists(int id)
        {
            return _context.Empresas.Any(e => e.Id == id);
        }
    }
}
