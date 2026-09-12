using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.ViewModel.Notificaciones;

namespace saas.ViewComponents
{
    public class NotificacionesViewComponent : ViewComponent
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IFechaHoraService _fechaHora;
        private readonly IRevisionNotificacionService _revisionNotificacionService;
        private readonly NotificacionNovedadService _notificacionNovedadService;

        public NotificacionesViewComponent(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            IFechaHoraService fechaHora,
            IRevisionNotificacionService revisionNotificacionService,
            NotificacionNovedadService notificacionNovedadService)
        {
            _context = context;
            _userManager = userManager;
            _fechaHora = fechaHora;
            _revisionNotificacionService = revisionNotificacionService;
            _notificacionNovedadService = notificacionNovedadService;
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
                .Select(p => new NotificacionStockItemVM
                {
                    ProductoId = p.Id,
                    EmpresaId = p.EmpresaId,
                    Producto = p.Nombre,
                    Empresa = p.Empresa.Nombre,
                    Stock = p.Stock,
                    PuntoReposicion = p.PuntoReposicion,
                    FechaOrigen = p.MovimientosStock
                        .Select(m => (DateTime?)m.Fecha)
                        .Max() ?? p.FechaAlta
                })
                .ToListAsync();

            DateTime inicioDia = _fechaHora.ConvertirAUtc(_fechaHora.FechaLocalHoy);
            DateTime finDia = _fechaHora.ConvertirAUtc(_fechaHora.FechaLocalHoy.AddDays(1));

            var consultaVentas = _context.Ventas
                .AsNoTracking()
                .Join(
                    _context.ConfiguracionesEmpresa
                        .AsNoTracking()
                        .Where(c => c.MontoVentaImportante.HasValue),
                    venta => venta.EmpresaId,
                    configuracion => configuracion.EmpresaId,
                    (venta, configuracion) => new
                    {
                        Venta = venta,
                        Configuracion = configuracion
                    })
                .Where(x =>
                    x.Venta.Estado &&
                    x.Venta.Fecha >= inicioDia &&
                    x.Venta.Fecha < finDia &&
                    x.Venta.Total >= x.Configuracion.MontoVentaImportante!.Value);

            if (!esSuperAdmin)
            {
                consultaVentas = consultaVentas.Where(x => x.Venta.EmpresaId == usuario.EmpresaId);
            }

            vm.CantidadVentasImportantes = await consultaVentas.CountAsync();

            vm.Ventas = await consultaVentas
                .OrderByDescending(x => x.Venta.Fecha)
                .ThenByDescending(x => x.Venta.Id)
                .Select(x => new NotificacionVentaItemVM
                {
                    VentaId = x.Venta.Id,
                    EmpresaId = x.Venta.EmpresaId,
                    Fecha = x.Venta.Fecha,
                    Cliente = x.Venta.Cliente == null
                        ? "Consumidor final"
                        : x.Venta.Cliente.Nombre +
                            (x.Venta.Cliente.Apellido == null ? "" : " " + x.Venta.Cliente.Apellido),
                    Empresa = x.Venta.Empresa.Nombre,
                    Total = x.Venta.Total
                })
                .ToListAsync();

            List<OrigenNotificacion> origenes = vm.Productos
                .Select(p => new OrigenNotificacion(p.EmpresaId, p.FechaOrigen))
                .Concat(vm.Ventas.Select(v =>
                    new OrigenNotificacion(v.EmpresaId, v.Fecha)))
                .ToList();

            int[] empresaIds = origenes
                .Select(x => x.EmpresaId)
                .Distinct()
                .ToArray();

            IReadOnlyDictionary<int, DateTime> revisiones =
                await _revisionNotificacionService.ObtenerUltimasRevisionesAsync(
                    usuario,
                    empresaIds,
                    esSuperAdmin);

            vm.CantidadNuevas =
                _notificacionNovedadService.CalcularCantidad(origenes, revisiones);

            return View(vm);
        }
    }
}
