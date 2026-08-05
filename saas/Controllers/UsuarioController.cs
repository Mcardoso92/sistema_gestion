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
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
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
        // GET: Usuario
        public async Task<IActionResult> Index(string estado = "activos", string? rol = null, int? empresaId = null, string? busqueda = null)
        {
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            IQueryable<Usuario> usuarios = _context.Users
                .AsNoTracking()
                .Include(u => u.Empresa);

            if (!esSuperAdmin)
            {
                empresaId = usuarioLogueado.EmpresaId;

                usuarios = usuarios.Where(u => u.EmpresaId == usuarioLogueado.EmpresaId);

                var rolSuperAdmin = await _roleManager.FindByNameAsync("SuperAdmin");

                if (rolSuperAdmin != null)
                {
                    IQueryable<string> idsSuperAdmin = _context.UserRoles
                        .Where(ur => ur.RoleId == rolSuperAdmin.Id)
                        .Select(ur => ur.UserId);

                    usuarios = usuarios.Where(u => !idsSuperAdmin.Contains(u.Id));
                }
            }
            else if (empresaId.HasValue)
            {
                usuarios = usuarios.Where(u => u.EmpresaId == empresaId.Value);
            }

            switch (estado.ToLower())
            {
                case "inactivos":
                    usuarios = usuarios.Where(u => !u.Estado);
                    break;

                case "todos":
                    break;

                default:
                    usuarios = usuarios.Where(u => u.Estado);
                    estado = "activos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                var rolDb = await _roleManager.FindByNameAsync(rol);

                if (rolDb != null)
                {
                    IQueryable<string> idsUsuariosRol = _context.UserRoles
                        .Where(ur => ur.RoleId == rolDb.Id)
                        .Select(ur => ur.UserId);

                    usuarios = usuarios.Where(u => idsUsuariosRol.Contains(u.Id));
                }
                else
                {
                    usuarios = usuarios.Where(u => false);
                }
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                usuarios = usuarios.Where(u =>
                    u.Nombre.Contains(busqueda) ||
                    u.Apellido.Contains(busqueda) ||
                    (u.Email != null && u.Email.Contains(busqueda)));
            }

            var listaUsuarios = await usuarios
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToListAsync();

            IQueryable<IdentityRole> rolesDisponibles = _roleManager.Roles
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                rolesDisponibles = rolesDisponibles.Where(r => r.Name != "SuperAdmin");
            }

            ViewBag.RolesDisponibles = await rolesDisponibles
                .OrderBy(r => r.Name)
                .ToListAsync();

            if (esSuperAdmin)
            {
                ViewBag.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
            }

            ViewBag.Estado = estado;
            ViewBag.Rol = rol;
            ViewBag.EmpresaId = esSuperAdmin ? empresaId : null;
            ViewBag.Busqueda = busqueda;
            ViewBag.Roles = await ObtenerRolesUsuarios(listaUsuarios);

            return View(listaUsuarios);
        }
        // GET: Producto/Details/5
        public async Task<IActionResult> Details(string? id)
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
                .Include(p => p.Empresa);

            if (!await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin"))
            {
                consulta = consulta.Where(c => c.EmpresaId == usuarioLogueado.EmpresaId);
            }
            var usuario = await consulta.FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuarioLogueado,
                "SuperAdmin");

            bool usuarioEsSuperAdmin = await _userManager.IsInRoleAsync(
                usuario,
                "SuperAdmin");

            if (!esSuperAdmin && usuarioEsSuperAdmin)
            {
                return Forbid();
            }

            var rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();

            ViewBag.Rol = rol;

            return View(usuario);
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

            IQueryable<IdentityRole> roles = _roleManager.Roles;

            if (!esSuperAdmin)
            {
                roles = roles.Where(r => r.Name != "SuperAdmin");
            }

            model.Roles = await roles
                .OrderBy(r => r.Name)
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

                if (!esSuperAdmin && usuario.Rol == "SuperAdmin")
                {
                    ModelState.AddModelError(
                        "Rol",
                        "No tiene permisos para asignar el rol SuperAdmin.");

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                bool existeRol = await _roleManager.RoleExistsAsync(usuario.Rol);

                if (!existeRol)
                {
                    ModelState.AddModelError(
                        "Rol",
                        "El rol seleccionado no es válido.");

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                var usuarioDb = new Usuario
                {
                    UserName = usuario.Email,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    EmpresaId = usuario.EmpresaId,
                    Estado = usuario.Estado,
                    FechaAlta = DateTime.Now
                };

                var resultado = await _userManager.CreateAsync(usuarioDb, usuario.Password);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }
                var resultadoRol = await _userManager.AddToRoleAsync(usuarioDb, usuario.Rol);

                if (!resultadoRol.Succeeded)
                {
                    foreach (var error in resultadoRol.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await _userManager.DeleteAsync(usuarioDb);

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

            var usuarioDb = await consulta.FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            bool usuarioEsSuperAdmin = await _userManager.IsInRoleAsync(usuarioDb, "SuperAdmin");

            if (!esSuperAdmin && usuarioEsSuperAdmin)
            {
                return Forbid();
            }

            var viewModel = new UsuarioEditVM
            {
                Id = usuarioDb.Id,
                Nombre = usuarioDb.Nombre,
                Apellido = usuarioDb.Apellido,
                Email = usuarioDb.Email!,
                Estado = usuarioDb.Estado,
                EmpresaId = usuarioDb.EmpresaId
            };

            // Obtengo el rol actual del usuario a editar
            viewModel.Rol = (await _userManager.GetRolesAsync(usuarioDb)).FirstOrDefault();


            // Cargo los combos
            await CargarCombos(viewModel, esSuperAdmin);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            UsuarioEditVM usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(
                usuarioLogueado,
                "SuperAdmin");

            var usuarioDb = await _userManager.FindByIdAsync(id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            if (!esSuperAdmin &&
                usuarioDb.EmpresaId != usuarioLogueado.EmpresaId)
            {
                return Forbid();
            }

            bool usuarioEsSuperAdmin = await _userManager.IsInRoleAsync(
                usuarioDb,
                "SuperAdmin");

            if (!esSuperAdmin && usuarioEsSuperAdmin)
            {
                return Forbid();
            }

            var rolesActuales = await _userManager.GetRolesAsync(usuarioDb);
            string? rolActual = rolesActuales.FirstOrDefault();

            // Un AdminEmpresa no recibe EmpresaId desde la vista.
            if (!esSuperAdmin)
            {
                usuario.EmpresaId = usuarioLogueado.EmpresaId;
                ModelState.Remove(nameof(usuario.EmpresaId));
            }

            // Un usuario no puede cambiar su propio rol.
            if (usuarioDb.Id == usuarioLogueado.Id)
            {
                usuario.Rol = rolActual!;
                ModelState.Remove(nameof(usuario.Rol));
            }

            if (!ModelState.IsValid)
            {
                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            if (!esSuperAdmin &&
                usuario.Rol == "SuperAdmin")
            {
                ModelState.AddModelError(
                    nameof(usuario.Rol),
                    "No tiene permisos para asignar el rol SuperAdmin.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            if (usuarioDb.Id == usuarioLogueado.Id &&
                !usuario.Estado)
            {
                ModelState.AddModelError(
                    nameof(usuario.Estado),
                    "No puede desactivar su propio usuario.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            var existeEmail = await _userManager.FindByEmailAsync(
                usuario.Email);

            if (existeEmail != null &&
                existeEmail.Id != usuario.Id)
            {
                ModelState.AddModelError(
                    nameof(usuario.Email),
                    "Ya existe un usuario con ese correo electrónico.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            bool existeRol = await _roleManager.RoleExistsAsync(usuario.Rol);

            if (!existeRol)
            {
                ModelState.AddModelError(
                    nameof(usuario.Rol),
                    "El rol seleccionado no es válido.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            try
            {
                usuarioDb.Nombre = usuario.Nombre;
                usuarioDb.Apellido = usuario.Apellido;
                usuarioDb.Email = usuario.Email;
                usuarioDb.UserName = usuario.Email;
                usuarioDb.Estado = usuario.Estado;

                if (esSuperAdmin)
                {
                    usuarioDb.EmpresaId = usuario.EmpresaId;
                }

                var resultado = await _userManager.UpdateAsync(usuarioDb);

                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                if (rolActual != usuario.Rol)
                {
                    if (rolActual != null)
                    {
                        var resultadoEliminarRol =
                            await _userManager.RemoveFromRoleAsync(
                                usuarioDb,
                                rolActual);

                        if (!resultadoEliminarRol.Succeeded)
                        {
                            foreach (var error in resultadoEliminarRol.Errors)
                            {
                                ModelState.AddModelError(
                                    "",
                                    error.Description);
                            }

                            await CargarCombos(usuario, esSuperAdmin);

                            return View(usuario);
                        }
                    }

                    var resultadoAgregarRol =
                        await _userManager.AddToRoleAsync(
                            usuarioDb,
                            usuario.Rol);

                    if (!resultadoAgregarRol.Succeeded)
                    {
                        if (rolActual != null)
                        {
                            await _userManager.AddToRoleAsync(
                                usuarioDb,
                                rolActual);
                        }

                        foreach (var error in resultadoAgregarRol.Errors)
                        {
                            ModelState.AddModelError(
                                "",
                                error.Description);
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
                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al modificar el usuario.");

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

            var usuarioDb = await consulta.FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            bool usuarioEsSuperAdmin = await _userManager.IsInRoleAsync(usuarioDb, "SuperAdmin");

            if (!esSuperAdmin && usuarioEsSuperAdmin)
            {
                return Forbid();
            }

            var rol = (await _userManager.GetRolesAsync(usuarioDb)).FirstOrDefault();

            var vm = new UsuarioDeleteVM
            {
                Id = usuarioDb.Id,
                Nombre = usuarioDb.Nombre,
                Apellido = usuarioDb.Apellido,
                Email = usuarioDb.Email!,
                Empresa = usuarioDb.Empresa.Nombre,
                Rol = rol ?? "",
                Estado = usuarioDb.Estado
            };

            return View(vm);
        }
        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
        public async Task<IActionResult> DeleteConfirmed(string id)
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

            var usuarioDb = await _userManager.FindByIdAsync(id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            // Si no es SuperAdmin, solo puede desactivar usuarios de su empresa.
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            if (!esSuperAdmin && usuarioDb.EmpresaId != usuarioLogueado.EmpresaId)
            {
                return Forbid();
            }

            // Un AdminEmpresa nunca puede desactivar un SuperAdmin.
            bool usuarioEsSuperAdmin = await _userManager.IsInRoleAsync(
                usuarioDb,
                "SuperAdmin");

            if (!esSuperAdmin && usuarioEsSuperAdmin)
            {
                return Forbid();
            }

            // No permitir que un usuario se desactive a sí mismo.
            if (usuarioDb.Id == usuarioLogueado.Id)
            {
                TempData["Error"] = "No puede desactivar su propio usuario.";

                return RedirectToAction(nameof(Index));
            }           

            try
            {
                usuarioDb.Estado = false;

                var resultado = await _userManager.UpdateAsync(usuarioDb);

                if (!resultado.Succeeded)
                {
                    TempData["Error"] = "Ocurrió un error al desactivar el usuario.";

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

            // Verificamos que la empresa del usuario se encuentre activa.
            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                bool empresaActiva = await _context.Empresas.AnyAsync(e =>
                    e.Id == usuario.EmpresaId &&
                    e.Estado);

                if (!empresaActiva)
                {
                    ModelState.AddModelError("", "La empresa se encuentra inactiva.");
                    return View(model);
                }
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
            IQueryable<IdentityRole> roles = _roleManager.Roles;

            if (!esSuperAdmin)
            {
                roles = roles.Where(r => r.Name != "SuperAdmin");
            }

            model.Roles = await roles
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
            IQueryable<IdentityRole> roles = _roleManager.Roles;

            if (!esSuperAdmin)
            {
                roles = roles.Where(r => r.Name != "SuperAdmin");
            }

            model.Roles = await roles
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

        private async Task<Dictionary<string, string>> ObtenerRolesUsuarios(IEnumerable<Usuario> usuarios)
        {
            var idsUsuarios = usuarios
                .Select(u => u.Id)
                .ToList();

            if (!idsUsuarios.Any())
            {
                return new Dictionary<string, string>();
            }

            var rolesUsuarios = await (
                from usuarioRol in _context.UserRoles
                join rol in _context.Roles on usuarioRol.RoleId equals rol.Id
                where idsUsuarios.Contains(usuarioRol.UserId)
                select new
                {
                    usuarioRol.UserId,
                    Rol = rol.Name
                })
                .AsNoTracking()
                .ToListAsync();

            return rolesUsuarios
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.First().Rol ?? "");
        }
    }
}
