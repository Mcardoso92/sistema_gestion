using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;
using System;

namespace saas.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly SaasDbContext _context;

        public VentaController(SaasDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Productos = _context.Productos
                .Where(p => p.Estado)
                .ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(VentaCreateVM model)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == model.ProductoId);

            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Stock < model.Cantidad)
            {
                ModelState.AddModelError("", "Stock insuficiente");

                ViewBag.Productos = _context.Productos
                    .Where(p => p.Estado)
                    .ToList();

                return View(model);
            }

            // ACA SE CREA LA VENTA
            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Total = producto.PrecioVenta * model.Cantidad,
                Estado = true,
                EmpresaId = producto.EmpresaId
            };

            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            return View();
        }
    }
}
