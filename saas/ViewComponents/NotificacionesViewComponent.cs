using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel.Notificaciones;

namespace saas.ViewComponents
{
    public class NotificacionesViewComponent : ViewComponent
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public NotificacionesViewComponent(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new NotificacionesVM();

            if (!(UserClaimsPrincipal.Identity?.IsAuthenticated ?? false))
            {
                return View(vm);
            }

            var usuario = await _userManager.GetUserAsync(UserClaimsPrincipal);

            if (usuario == null)
            {
                return View(vm);
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            ViewBag.EsSuperAdmin = esSuperAdmin;

            IQueryable<Producto> consulta = _context.Productos
                .AsNoTracking()
                .Where(p => p.Estado && p.Stock <= p.PuntoReposicion);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(p => p.EmpresaId == usuario.EmpresaId);
            }

            vm.CantidadSinStock = await consulta.CountAsync(p => p.Stock == 0);
            vm.CantidadStockBajo = await consulta.CountAsync(p => p.Stock > 0);

            vm.Productos = await consulta
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Nombre)
                .Take(6)
                .Select(p => new NotificacionStockItemVM
                {
                    ProductoId = p.Id,
                    Producto = p.Nombre,
                    Empresa = p.Empresa.Nombre,
                    Stock = p.Stock,
                    PuntoReposicion = p.PuntoReposicion
                })
                .ToListAsync();

            return View(vm);
        }
    }
}