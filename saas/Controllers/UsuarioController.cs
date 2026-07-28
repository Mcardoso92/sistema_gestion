using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;

namespace saas.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SaasDbContext _context;
        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, RoleManager<IdentityRole> roleManager, SaasDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        // GET: Producto
        [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Usuario> usuarios = _context.Users
                .Where(u => u.Estado)
                .Include(u => u.Empresa);


            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            // Si es SuperAdmin ve todos los usuarios, de lo contrario solo ve los de su empresa
            if (!esSuperAdmin)
            {
                // Si no es SuperAdmin, solo ve los de su empresa
                usuarios = usuarios.Where(u =>  u.EmpresaId == usuario.EmpresaId);
            }

            var listaUsuarios = await usuarios
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.Roles = new Dictionary<string, string>();

            foreach (var usuarioItem in listaUsuarios)
            {
                var rol = await _userManager.GetRolesAsync(usuarioItem);
                ViewBag.Roles[usuarioItem.Id] = rol.FirstOrDefault() ?? "";
            }

            return View(listaUsuarios);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _userManager.FindByEmailAsync(model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }

            if (!usuario.Estado)
            {
                ModelState.AddModelError("", "El usuario se encuentra inactivo.");
                return View(model);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
                {
                    return RedirectToAction("Index", "Empresa");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
