using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using saas.Models;
using saas.Services;
using saas.ViewModel.CuentaCorriente;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class CuentaCorrienteController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly CuentaCorrienteService _service;

        public CuentaCorrienteController(
            UserManager<Usuario> userManager,
            CuentaCorrienteService service)
        {
            _userManager = userManager;
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Cliente(
            int id,
            string estado = "todos",
            int pagina = 1)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            CuentaCorrienteVM? modelo = await _service.ObtenerClienteAsync(
                id,
                esSuperAdmin ? null : usuario.EmpresaId,
                estado,
                pagina);

            return modelo == null ? NotFound() : View("Index", modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Proveedor(
            int id,
            string estado = "todos",
            int pagina = 1)
        {
            Usuario? usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return Challenge();

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");
            CuentaCorrienteVM? modelo = await _service.ObtenerProveedorAsync(
                id,
                esSuperAdmin ? null : usuario.EmpresaId,
                estado,
                pagina);

            return modelo == null ? NotFound() : View("Index", modelo);
        }
    }
}
