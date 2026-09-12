using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;

namespace saas.Controllers
{
    [Authorize]
    public class NotificacionController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IRevisionNotificacionService _revisionNotificacionService;

        public NotificacionController(
            SaasDbContext context,
            UserManager<Usuario> userManager,
            IRevisionNotificacionService revisionNotificacionService)
        {
            _context = context;
            _userManager = userManager;
            _revisionNotificacionService = revisionNotificacionService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarComoVistas()
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Unauthorized();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            int[] empresaIds = esSuperAdmin
                ? await _context.Empresas
                    .AsNoTracking()
                    .Select(e => e.Id)
                    .ToArrayAsync()
                : new[] { usuario.EmpresaId };

            await _revisionNotificacionService.RegistrarRevisionesAsync(
                usuario,
                empresaIds,
                esSuperAdmin);

            return NoContent();
        }
    }
}
