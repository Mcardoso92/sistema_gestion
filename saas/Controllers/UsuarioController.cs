using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.ViewModel;

namespace saas.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, SaasDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string user)
        {
            return View();
        }
        public IActionResult Registro()
        {
            var vm = new RegistroVM
            {
                Empresas = _context.Empresas
                    .Where(e => e.Estado)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
            };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroVM nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                //Logica para registrar al usuario
                var usuario = new Usuario
                {
                    UserName = nuevoUsuario.Email,
                    Email = nuevoUsuario.Email,
                    Nombre = nuevoUsuario.Nombre,
                    Apellido = nuevoUsuario.Apellido,
                    EmpresaId = nuevoUsuario.EmpresaId,
                    ImagenPerfil = "default-profile.png",
                };
                var resultado = await _userManager.CreateAsync(usuario, nuevoUsuario.Clave);
                if (resultado.Succeeded)
                {
                    //Asignar rol al usuario
                    await _userManager.AddToRoleAsync(usuario, "Usuario");
                    //Redirigir a la página de login o a otra página
                    return RedirectToAction("Login", "Usuario");
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

            }
            return View();
        }

        public IActionResult Logout()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
