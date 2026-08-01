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
        // GET: Producto/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            IQueryable<Usuario> consulta = _context.Users
                .Include(p => p.Empresa);

            if (!await _userManager.IsInRoleAsync(usuario, "SuperAdmin"))
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }
            var detalleUsuario = await consulta.FirstOrDefaultAsync(m => m.Id == id.ToString());

            if (detalleUsuario == null)
            {
                return NotFound();
            }

            var rol = (await _userManager.GetRolesAsync(detalleUsuario)).FirstOrDefault();

            ViewBag.Rol = rol;

            return View(detalleUsuario);
        }
        // GET: Producto/Create
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }
            var model = new UsuarioCreateVM();

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (esSuperAdmin)
            {
                model.Empresas = await _context.Empresas
                    .Where(e => e.Estado)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
                    .ToListAsync();
            }
            else
            {
                model.Empresas = await _context.Empresas
                    .Where(e => e.Id == usuario.EmpresaId)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
                    .ToListAsync();
            }
            model.Roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name!
                })
                .ToListAsync();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateVM usuario)
        {
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            try
            {
                // Si no es SuperAdmin, la empresa siempre es la del usuario
                if (!esSuperAdmin)
                {
                    usuario.EmpresaId = usuarioLogueado.EmpresaId;
                }

                if (!ModelState.IsValid)
                {
                    await CargarCombos(usuario, esSuperAdmin);
                    return View(usuario);
                }

                var existeUsuario = await _userManager.FindByEmailAsync(usuario.Email);

                if (existeUsuario != null)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con ese correo electrónico.");

                    await CargarCombos(usuario, esSuperAdmin);
                    return View(usuario);
                }

                var nuevoUsuario = new Usuario
                {
                    UserName = usuario.Email,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    EmpresaId = usuario.EmpresaId,
                    Estado = usuario.Estado,
                    FechaAlta = DateTime.Now
                };

                var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Password);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }
                var resultadoRol = await _userManager.AddToRoleAsync(nuevoUsuario, usuario.Rol);

                if (!resultadoRol.Succeeded)
                {
                    foreach (var error in resultadoRol.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                TempData["Success"] = "Usuario creado correctamente.";

                return RedirectToAction(nameof(Index));

            }
            catch
            {

                ModelState.AddModelError("", "Ocurrió un error al crear el usuario.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }
        }
        // GET: Producto/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            IQueryable<Usuario> consulta = _context.Users
                .Include(u => u.Empresa);

            // Si no es SuperAdmin, solo puede editar usuarios de su empresa
            if (!esSuperAdmin)
            {
                consulta = consulta.Where(u => u.EmpresaId == usuarioLogueado.EmpresaId);
            }

            var usuario = await consulta.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new UsuarioEditVM
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email!,
                Estado = usuario.Estado,
                EmpresaId = usuario.EmpresaId
            };

            // Obtengo el rol actual del usuario a editar
            viewModel.Rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();


            // Cargo los combos
            await CargarCombos(viewModel, esSuperAdmin);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsuarioEditVM usuario)
        {
            // Verificamos que el id de la URL coincida con el del ViewModel.
            // Esto evita que alguien modifique el formulario manualmente.
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Obtenemos el usuario logueado.
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            // Si el modelo tiene errores de validación,
            // recargamos los combos y volvemos a mostrar la vista.
            if (!ModelState.IsValid)
            {
                await CargarCombos(usuario, esSuperAdmin);
                return View(usuario);
            }

            try
            {
                // Buscamos el usuario que queremos editar.
                var usuarioEditar = await _userManager.FindByIdAsync(id);

                if (usuarioEditar == null)
                {
                    return NotFound();
                }

                // Si NO es SuperAdmin solo puede modificar usuarios
                // de su propia empresa.
                if (!esSuperAdmin &&
                    usuarioEditar.EmpresaId != usuarioLogueado.EmpresaId)
                {
                    return Forbid();
                }

                // Verificamos que el nuevo email no exista en otro usuario.
                var existeEmail = await _userManager.FindByEmailAsync(usuario.Email);

                if (existeEmail != null && existeEmail.Id != usuario.Id)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con ese correo electrónico.");

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                // Actualizamos únicamente los campos permitidos.
                usuarioEditar.Nombre = usuario.Nombre;
                usuarioEditar.Apellido = usuario.Apellido;
                usuarioEditar.Email = usuario.Email;
                usuarioEditar.UserName = usuario.Email;
                usuarioEditar.Estado = usuario.Estado;

                // Solo el SuperAdmin puede cambiar la empresa.
                if (esSuperAdmin)
                {
                    usuarioEditar.EmpresaId = usuario.EmpresaId;
                }

                // Guardamos los cambios básicos.
                var resultado = await _userManager.UpdateAsync(usuarioEditar);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                // ============================
                // Actualización del Rol
                // ============================

                // Obtenemos el rol actual.
                var rolesActuales = await _userManager.GetRolesAsync(usuarioEditar);

                var rolActual = rolesActuales.FirstOrDefault();

                // Solo actualizamos si cambió.
                if (rolActual != usuario.Rol)
                {
                    if (rolActual != null)
                    {
                        await _userManager.RemoveFromRoleAsync(usuarioEditar, rolActual);
                    }

                    var resultadoRol = await _userManager.AddToRoleAsync(usuarioEditar, usuario.Rol);

                    if (!resultadoRol.Succeeded)
                    {
                        foreach (var error in resultadoRol.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        await CargarCombos(usuario, esSuperAdmin);

                        return View(usuario);
                    }
                }

                TempData["Success"] = "Usuario modificado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al modificar el usuario.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }
        }
        // GET: Usuario/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            IQueryable<Usuario> consulta = _context.Users
                .Include(u => u.Empresa);

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(u => u.EmpresaId == usuarioLogueado.EmpresaId);
            }

            var usuario = await consulta.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            var rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();

            var vm = new UsuarioDeleteVM
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email!,
                Empresa = usuario.Empresa.Nombre,
                Rol = rol ?? "",
                Estado = usuario.Estado
            };

            return View(vm);
        }
        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            var usuario = await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // No permitir que un usuario se elimine a sí mismo.
            if (usuario.Id == usuarioLogueado.Id)
            {
                TempData["Error"] = "No puede desactivar su propio usuario.";

                return RedirectToAction(nameof(Index));
            }

            // Si no es SuperAdmin, sólo puede eliminar usuarios de su empresa.
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            if (!esSuperAdmin && usuario.EmpresaId != usuarioLogueado.EmpresaId)
            {
                return Forbid();
            }

            try
            {
                usuario.Estado = false;

                var resultado = await _userManager.UpdateAsync(usuario);

                if (!resultado.Succeeded)
                {
                    TempData["Error"] = "No fue posible desactivar el usuario.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Usuario desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al desactivar el usuario.";

                return RedirectToAction(nameof(Index));
            }
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

        private async Task CargarCombos(UsuarioCreateVM model, bool esSuperAdmin)
        {
            model.Roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                model.Empresas = await _context.Empresas
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
                    .ToListAsync();
            }
        }
        private async Task CargarCombos(UsuarioEditVM model, bool esSuperAdmin)
        {
            model.Roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name
                })
                .ToListAsync();

            if (esSuperAdmin)
            {
                model.Empresas = await _context.Empresas
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.Nombre
                    })
                    .ToListAsync();
            }
        }
    }
}
