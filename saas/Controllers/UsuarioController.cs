using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Services;
using saas.Settings;
using saas.ViewModel;
using saas.ViewModel.Autenticacion;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class UsuarioController : VeltikaController
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SaasDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IImagenService _imagenService;
        private readonly EmpresaInicializacionService _empresaInicializacionService;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<UsuarioController> _logger;
        public UsuarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<IdentityRole> roleManager,
            SaasDbContext context,
            IEmailService emailService,
            IImagenService imagenService,
            EmpresaInicializacionService empresaInicializacionService,
            IOptions<EmailSettings> emailOptions,
            ILogger<UsuarioController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
            _imagenService = imagenService;
            _empresaInicializacionService = empresaInicializacionService;
            _emailSettings = emailOptions.Value;
            _logger = logger;
        }
        // GET: Usuario
        public async Task<IActionResult> Index(string estado = "activos", string? rol = null, int? empresaId = null, string? busqueda = null, int pagina = 1)
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

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalUsuarios = await usuarios.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalUsuarios / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalUsuarios;

            var listaUsuarios = await usuarios
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
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

            string? rutaImagenNueva = null;
            Usuario? usuarioCreado = null;

            try
            {
                if (!esSuperAdmin)
                {
                    usuario.EmpresaId = usuarioLogueado.EmpresaId;
                    ModelState.Remove(nameof(usuario.EmpresaId));
                }

                if (!ModelState.IsValid)
                {
                    await CargarCombos(usuario, esSuperAdmin);
                    return View(usuario);
                }

                bool empresaValida = await _context.Empresas.AnyAsync(e =>
                    e.Id == usuario.EmpresaId &&
                    e.Estado);

                if (!empresaValida)
                {
                    ModelState.AddModelError(
                        nameof(usuario.EmpresaId),
                        "La empresa seleccionada no es válida o se encuentra inactiva.");

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

                if (!esSuperAdmin && string.Equals(usuario.Rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
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
                        string campo = error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                            ? nameof(usuario.Password)
                            : error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase) ||
                              error.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase)
                                ? nameof(usuario.Email)
                                : string.Empty;

                        ModelState.AddModelError(campo, error.Description);
                    }

                    await CargarCombos(usuario, esSuperAdmin);

                    return View(usuario);
                }

                usuarioCreado = usuarioDb;

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

                if (usuario.ImagenArchivo != null)
                {
                    ResultadoImagen resultadoImagen = await _imagenService.GuardarAsync(usuario.ImagenArchivo, usuarioDb.EmpresaId, "usuarios", usuarioDb.Id);

                    if (!resultadoImagen.Exito)
                    {
                        ModelState.AddModelError(nameof(usuario.ImagenArchivo), resultadoImagen.Error!);
                        await _userManager.DeleteAsync(usuarioDb);
                        await CargarCombos(usuario, esSuperAdmin);

                        return View(usuario);
                    }

                    rutaImagenNueva = resultadoImagen.Ruta;
                    usuarioDb.ImagenPerfil = rutaImagenNueva;

                    var resultadoImagenUsuario = await _userManager.UpdateAsync(usuarioDb);

                    if (!resultadoImagenUsuario.Succeeded)
                    {
                        _imagenService.Eliminar(rutaImagenNueva);
                        await _userManager.DeleteAsync(usuarioDb);

                        foreach (var error in resultadoImagenUsuario.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        await CargarCombos(usuario, esSuperAdmin);

                        return View(usuario);
                    }
                }

                TempData["Success"] = "Usuario creado correctamente.";

                return RedirectToAction(nameof(Index));

            }
            catch
            {
                _imagenService.Eliminar(rutaImagenNueva);

                if (usuarioCreado != null)
                {
                    await _userManager.DeleteAsync(usuarioCreado);
                }

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
                EmpresaId = usuarioDb.EmpresaId,
                ImagenActual = usuarioDb.ImagenPerfil,
                EsUsuarioLogueado = usuarioDb.Id == usuarioLogueado.Id
            };

            // Obtengo el rol actual del usuario a editar
            viewModel.Rol = (await _userManager.GetRolesAsync(usuarioDb)).FirstOrDefault() ?? string.Empty;

            viewModel.MotivoBloqueoDesactivacion =
                await ObtenerMotivoBloqueoCambioAdministradorAsync(
                    usuarioDb,
                    usuarioLogueado,
                    false,
                    viewModel.Rol,
                    usuarioDb.EmpresaId);
            viewModel.PuedeDesactivar =
                viewModel.MotivoBloqueoDesactivacion == null;


            // Cargo los combos
            await CargarCombos(viewModel, esSuperAdmin);

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UsuarioEditVM usuario)
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
                usuario.EmpresaId = usuarioDb.EmpresaId;

                ModelState.Remove(nameof(usuario.Rol));
                ModelState.Remove(nameof(usuario.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == usuario.EmpresaId &&
                e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError(
                    nameof(usuario.EmpresaId),
                    "La empresa seleccionada no es válida o se encuentra inactiva.");

                await CargarCombos(usuario, esSuperAdmin);
                return View(usuario);
            }

            if (!esSuperAdmin && string.Equals(usuario.Rol, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(usuario.Rol),
                    "No tiene permisos para asignar el rol SuperAdmin.");

                await CargarCombos(usuario, esSuperAdmin);

                return View(usuario);
            }

            string? motivoBloqueo =
                await ObtenerMotivoBloqueoCambioAdministradorAsync(
                    usuarioDb,
                    usuarioLogueado,
                    usuario.Estado,
                    usuario.Rol,
                    usuario.EmpresaId);

            if (motivoBloqueo != null)
            {
                ModelState.AddModelError(
                    nameof(usuario.Estado),
                    motivoBloqueo);

                usuario.Estado = usuarioDb.Estado;
                usuario.PuedeDesactivar = false;
                usuario.MotivoBloqueoDesactivacion = motivoBloqueo;

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

            string? rutaAnterior = usuarioDb.ImagenPerfil;
            string? rutaNueva = null;
            usuario.ImagenActual = rutaAnterior;

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                if (usuario.ImagenArchivo != null)
                {
                    ResultadoImagen resultadoImagen = await _imagenService.GuardarAsync(usuario.ImagenArchivo, usuario.EmpresaId, "usuarios", usuarioDb.Id);

                    if (!resultadoImagen.Exito)
                    {
                        ModelState.AddModelError(nameof(usuario.ImagenArchivo), resultadoImagen.Error!);
                        await CargarCombos(usuario, esSuperAdmin);

                        return View(usuario);
                    }

                    rutaNueva = resultadoImagen.Ruta;
                    usuarioDb.ImagenPerfil = rutaNueva;
                }
                else if (usuario.EliminarImagen)
                {
                    usuarioDb.ImagenPerfil = null;
                }

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

                    _imagenService.Eliminar(rutaNueva);

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

                            _imagenService.Eliminar(rutaNueva);

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

                        _imagenService.Eliminar(rutaNueva);

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

                await transaction.CommitAsync();

                if (rutaNueva != null || usuario.EliminarImagen)
                {
                    _imagenService.Eliminar(rutaAnterior);
                }

                TempData["Success"] = "Usuario modificado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _imagenService.Eliminar(rutaNueva);
                usuario.ImagenActual = rutaAnterior;

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

            vm.MotivoBloqueoDesactivacion =
                await ObtenerMotivoBloqueoCambioAdministradorAsync(
                    usuarioDb,
                    usuarioLogueado,
                    false,
                    rol ?? string.Empty,
                    usuarioDb.EmpresaId);
            vm.PuedeDesactivar =
                vm.MotivoBloqueoDesactivacion == null;

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

            string? motivoBloqueo =
                await ObtenerMotivoBloqueoCambioAdministradorAsync(
                    usuarioDb,
                    usuarioLogueado,
                    false,
                    (await _userManager.GetRolesAsync(usuarioDb)).FirstOrDefault()
                        ?? string.Empty,
                    usuarioDb.EmpresaId);

            if (motivoBloqueo != null)
            {
                TempData["Error"] = motivoBloqueo;

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
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("autenticacion")]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroEmpresaVM model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Apellido = model.Apellido.Trim();
            model.Email = model.Email.Trim();
            model.EmpresaNombre = model.EmpresaNombre.Trim();

            bool existeEmail = await _userManager.FindByEmailAsync(model.Email) != null;
            bool existeEmpresa = await _context.Empresas.AnyAsync(e => e.Nombre.ToLower() == model.EmpresaNombre.ToLower());

            if (existeEmail || existeEmpresa)
            {
                ModelState.AddModelError("", "No fue posible completar el registro con los datos ingresados.");
                return View(model);
            }

            Usuario? usuarioCreado = null;
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                DateTime fechaAlta = DateTime.Now;

                var empresa = new Empresa
                {
                    Nombre = model.EmpresaNombre,
                    Estado = true,
                    FechaAlta = fechaAlta
                };

                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();

                await _empresaInicializacionService.InicializarAsync(empresa.Id, fechaAlta);

                usuarioCreado = new Usuario
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    EmpresaId = empresa.Id,
                    Estado = true,
                    FechaAlta = fechaAlta
                };

                var resultadoUsuario = await _userManager.CreateAsync(usuarioCreado, model.Password);

                if (!resultadoUsuario.Succeeded)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "No fue posible crear el usuario. Revisá los datos y la contraseña.");
                    return View(model);
                }

                var resultadoRol = await _userManager.AddToRoleAsync(usuarioCreado, "AdminEmpresa");

                if (!resultadoRol.Succeeded)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "No fue posible completar el registro.");
                    return View(model);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ocurrió un error al crear la cuenta. Intentá nuevamente.");
                return View(model);
            }

            // La notificación es secundaria: un problema de correo no debe deshacer un registro confirmado.
            try
            {
                string empresaSegura = HtmlEncoder.Default.Encode(model.EmpresaNombre);
                string administradorSeguro = HtmlEncoder.Default.Encode($"{model.Nombre} {model.Apellido}");
                string emailSeguro = HtmlEncoder.Default.Encode(model.Email);

                var contenidoHtml = $"""
                    <h2>Nueva empresa registrada en Veltika</h2>
                    <p>Se completó un nuevo registro desde la aplicación.</p>
                    <ul>
                        <li><strong>Empresa:</strong> {empresaSegura}</li>
                        <li><strong>Administrador:</strong> {administradorSeguro}</li>
                        <li><strong>Correo:</strong> {emailSeguro}</li>
                        <li><strong>Fecha:</strong> {usuarioCreado!.FechaAlta:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    """;

                await _emailService.EnviarAsync(
                    _emailSettings.NotificationEmail,
                    $"Nueva empresa registrada: {model.EmpresaNombre}",
                    contenidoHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No fue posible enviar la notificación del registro de la empresa {EmpresaId}.", usuarioCreado!.EmpresaId);
            }

            await _signInManager.SignInAsync(usuarioCreado, isPersistent: false);

            return RedirectToAction("Index", "Dashboard");
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("autenticacion")]
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
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
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
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
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
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult RecuperarPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("autenticacion")]
        [AllowAnonymous]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (usuario != null &&
                usuario.Estado)
            {
                var token =
                    await _userManager
                        .GeneratePasswordResetTokenAsync(
                            usuario);

                var enlace =
                    Url.Action(
                        nameof(RestablecerPassword),
                        "Usuario",
                        new
                        {
                            email = usuario.Email,
                            token
                        },
                        Request.Scheme);

                if (enlace != null)
                {
                    var enlaceSeguro =
                        HtmlEncoder.Default.Encode(
                            enlace);

                    var contenidoHtml = $"""
                <p>Recibimos una solicitud para restablecer tu contraseña de Veltika.</p>
                <p>
                    <a href="{enlaceSeguro}">
                        Restablecer contraseña
                    </a>
                </p>
                <p>Si no realizaste esta solicitud, podés ignorar este mensaje.</p>
                """;

                    await _emailService.EnviarAsync(
                        usuario.Email!,
                        "Restablecer contraseña - Veltika",
                        contenidoHtml);
                }
            }

            ViewData["SolicitudEnviada"] =
                true;

            return View();
        }
        [AllowAnonymous]
        public IActionResult RestablecerPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var model =
                new RestablecerPasswordVM
                {
                    Email = email,
                    Token = token
                };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (usuario == null ||
                !usuario.Estado)
            {
                ModelState.AddModelError(
                    "",
                    "El enlace no es válido o ha expirado.");

                return View(model);
            }

            var resultado =
                await _userManager.ResetPasswordAsync(
                    usuario,
                    model.Token,
                    model.Password);

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    "El enlace no es válido o ha expirado.");

                return View(model);
            }

            TempData["Success"] =
                "La contraseña se restableció correctamente. Ya podés iniciar sesión.";

            return RedirectToAction(
                nameof(Login));
        }
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            CambiarPasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario =
                await _userManager.GetUserAsync(
                    User);

            if (usuario == null)
            {
                return Challenge();
            }

            var resultado =
                await _userManager.ChangePasswordAsync(
                    usuario,
                    model.PasswordActual,
                    model.PasswordNueva);

            if (!resultado.Succeeded)
            {
                if (resultado.Errors.Any(
                    error =>
                        error.Code == "PasswordMismatch"))
                {
                    ModelState.AddModelError(
                        nameof(model.PasswordActual),
                        "La contraseña actual es incorrecta.");
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "No fue posible cambiar la contraseña.");
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(
                usuario);

            TempData["Success"] =
                "La contraseña se cambió correctamente.";

            return RedirectToAction(
                nameof(Index));
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

        //Helpers Methods
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

        private async Task<string?> ObtenerMotivoBloqueoCambioAdministradorAsync(
            Usuario usuarioDb,
            Usuario usuarioLogueado,
            bool nuevoEstado,
            string nuevoRol,
            int nuevaEmpresaId)
        {
            if (usuarioDb.Id == usuarioLogueado.Id &&
                usuarioDb.Estado &&
                !nuevoEstado)
            {
                return "No puede desactivar su propio usuario.";
            }

            bool esAdminEmpresaActual =
                await _userManager.IsInRoleAsync(
                    usuarioDb,
                    "AdminEmpresa");

            bool conservaAdministracion =
                nuevoEstado &&
                nuevaEmpresaId == usuarioDb.EmpresaId &&
                string.Equals(
                    nuevoRol,
                    "AdminEmpresa",
                    StringComparison.OrdinalIgnoreCase);

            if (!usuarioDb.Estado ||
                !esAdminEmpresaActual ||
                conservaAdministracion)
            {
                return null;
            }

            var rolAdminEmpresa =
                await _roleManager.FindByNameAsync(
                    "AdminEmpresa");

            if (rolAdminEmpresa == null)
            {
                return "No fue posible verificar los administradores de la empresa.";
            }

            bool existeOtroAdministradorActivo =
                await (
                    from otroUsuario in _context.Users
                    join usuarioRol in _context.UserRoles
                        on otroUsuario.Id equals usuarioRol.UserId
                    where otroUsuario.Id != usuarioDb.Id &&
                          otroUsuario.EmpresaId == usuarioDb.EmpresaId &&
                          otroUsuario.Estado &&
                          usuarioRol.RoleId == rolAdminEmpresa.Id
                    select otroUsuario.Id)
                    .AnyAsync();

            return existeOtroAdministradorActivo
                ? null
                : "La empresa debe conservar al menos un administrador activo.";
        }
    }
}
