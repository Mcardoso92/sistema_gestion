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

        // GET: Venta
        public IActionResult Index()
        {
            return View();
        }

        // GET: Venta/Create
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new VentaCreateVM();

            return View(vm);
        }

        // POST: Venta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Implementaremos la lógica de la venta más adelante.

            return RedirectToAction(nameof(Index));
        }

        // GET: Venta/Details/5
        public IActionResult Details(int id)
        {
            return View();
        }
    }
}