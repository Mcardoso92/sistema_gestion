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
    public class ClienteController : VeltikaController
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ClienteController(SaasDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cliente
        public async Task<IActionResult> Index(string estado = "activos", int? empresaId = null, string? busqueda = null, int pagina = 1)
        {
            var usuarioLogueado = await _userManager.GetUserAsync(User);

            if (usuarioLogueado == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuarioLogueado, "SuperAdmin");

            IQueryable<Cliente> clientes = _context.Clientes
                .AsNoTracking()
                .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                empresaId = usuarioLogueado.EmpresaId;

                clientes = clientes.Where(c => c.EmpresaId == usuarioLogueado.EmpresaId);
            }
            else if (empresaId.HasValue)
            {
                clientes = clientes.Where(c => c.EmpresaId == empresaId.Value);
            }

            switch (estado.ToLower())
            {
                case "inactivos":
                    clientes = clientes.Where(c => !c.Estado);
                    break;

                case "todos":
                    break;

                default:
                    clientes = clientes.Where(c => c.Estado);
                    estado = "activos";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.Trim();

                clientes = clientes.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    (c.Apellido != null && c.Apellido.Contains(busqueda)) ||
                    (c.Documento != null && c.Documento.Contains(busqueda)) ||
                    (c.Email != null && c.Email.Contains(busqueda)));
            }

            if (esSuperAdmin)
            {
                ViewBag.Empresas = await _context.Empresas
                    .AsNoTracking()
                    .Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToListAsync();
            }

            ViewBag.Estado = estado;
            ViewBag.EmpresaId = esSuperAdmin ? empresaId : null;
            ViewBag.Busqueda = busqueda;

            const int tamanioPagina = 20;
            pagina = Math.Max(pagina, 1);
            int totalClientes = await clientes.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalClientes / (double)tamanioPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalClientes;

            var listaClientes = await clientes
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            return View(listaClientes);
        }

        // GET: Cliente/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking()
                .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var cliente = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Cliente/Create
        public async Task<IActionResult> Create()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            var clienteVM = new ClienteCreateVM();

            if (esSuperAdmin)
            {
                await CargarEmpresas(clienteVM);
            }
            else
            {
                clienteVM.EmpresaId = usuario.EmpresaId;
            }

            return View(clienteVM);
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteCreateVM clienteVM)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                clienteVM.EmpresaId = usuario.EmpresaId;
                ModelState.Remove(nameof(clienteVM.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == clienteVM.EmpresaId &&
                e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError("EmpresaId", "La empresa seleccionada no es válida.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }

            clienteVM.Nombre = clienteVM.Nombre.Trim();
            clienteVM.Apellido = string.IsNullOrWhiteSpace(clienteVM.Apellido)
                ? null
                : clienteVM.Apellido.Trim();
            clienteVM.Documento = string.IsNullOrWhiteSpace(clienteVM.Documento)
                ? null
                : clienteVM.Documento.Trim();
            clienteVM.Email = string.IsNullOrWhiteSpace(clienteVM.Email)
                ? null
                : clienteVM.Email.Trim();
            clienteVM.Telefono = string.IsNullOrWhiteSpace(clienteVM.Telefono)
                ? null
                : clienteVM.Telefono.Trim();
            clienteVM.Direccion = string.IsNullOrWhiteSpace(clienteVM.Direccion)
                ? null
                : clienteVM.Direccion.Trim();

            if (clienteVM.Documento != null)
            {
                bool existeDocumento = await _context.Clientes.AnyAsync(c =>
                    c.EmpresaId == clienteVM.EmpresaId &&
                    c.Documento == clienteVM.Documento);

                if (existeDocumento)
                {
                    ModelState.AddModelError(
                        "Documento",
                        "Ya existe un cliente con ese documento para esta empresa.");

                    if (esSuperAdmin)
                    {
                        await CargarEmpresas(clienteVM);
                    }

                    return View(clienteVM);
                }
            }

            try
            {
                var cliente = new Cliente
                {
                    Nombre = clienteVM.Nombre,
                    Apellido = clienteVM.Apellido,
                    Documento = clienteVM.Documento,
                    Email = clienteVM.Email,
                    Telefono = clienteVM.Telefono,
                    Direccion = clienteVM.Direccion,
                    EmpresaId = clienteVM.EmpresaId,
                    Estado = true,
                    FechaAlta = DateTime.Now
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cliente creado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al crear el cliente.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }
        }

        // GET: Cliente/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var cliente = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            var clienteVM = new ClienteEditVM
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Estado = cliente.Estado,
                EmpresaId = cliente.EmpresaId
            };

            if (esSuperAdmin)
            {
                await CargarEmpresas(clienteVM);
            }

            return View(clienteVM);
        }

        // POST: Cliente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteEditVM clienteVM)
        {
            if (id != clienteVM.Id)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            if (!esSuperAdmin)
            {
                clienteVM.EmpresaId = usuario.EmpresaId;
                ModelState.Remove(nameof(clienteVM.EmpresaId));
            }

            if (!ModelState.IsValid)
            {
                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }

            bool empresaValida = await _context.Empresas.AnyAsync(e =>
                e.Id == clienteVM.EmpresaId &&
                e.Estado);

            if (!empresaValida)
            {
                ModelState.AddModelError("EmpresaId", "La empresa seleccionada no es válida.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }

            clienteVM.Nombre = clienteVM.Nombre.Trim();
            clienteVM.Apellido = string.IsNullOrWhiteSpace(clienteVM.Apellido)
                ? null
                : clienteVM.Apellido.Trim();
            clienteVM.Documento = string.IsNullOrWhiteSpace(clienteVM.Documento)
                ? null
                : clienteVM.Documento.Trim();
            clienteVM.Email = string.IsNullOrWhiteSpace(clienteVM.Email)
                ? null
                : clienteVM.Email.Trim();
            clienteVM.Telefono = string.IsNullOrWhiteSpace(clienteVM.Telefono)
                ? null
                : clienteVM.Telefono.Trim();
            clienteVM.Direccion = string.IsNullOrWhiteSpace(clienteVM.Direccion)
                ? null
                : clienteVM.Direccion.Trim();

            if (clienteVM.Documento != null)
            {
                bool existeDocumento = await _context.Clientes.AnyAsync(c =>
                    c.Id != clienteVM.Id &&
                    c.EmpresaId == clienteVM.EmpresaId &&
                    c.Documento == clienteVM.Documento);

                if (existeDocumento)
                {
                    ModelState.AddModelError(
                        "Documento",
                        "Ya existe un cliente con ese documento para esta empresa.");

                    if (esSuperAdmin)
                    {
                        await CargarEmpresas(clienteVM);
                    }

                    return View(clienteVM);
                }
            }

            IQueryable<Cliente> consulta = _context.Clientes;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var clienteDb = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (clienteDb == null)
            {
                return NotFound();
            }

            try
            {
                clienteDb.Nombre = clienteVM.Nombre;
                clienteDb.Apellido = clienteVM.Apellido;
                clienteDb.Documento = clienteVM.Documento;
                clienteDb.Email = clienteVM.Email;
                clienteDb.Telefono = clienteVM.Telefono;
                clienteDb.Direccion = clienteVM.Direccion;
                clienteDb.Estado = clienteVM.Estado;

                if (esSuperAdmin)
                {
                    clienteDb.EmpresaId = clienteVM.EmpresaId;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cliente modificado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al modificar el cliente.");

                if (esSuperAdmin)
                {
                    await CargarEmpresas(clienteVM);
                }

                return View(clienteVM);
            }
        }

        // GET: Cliente/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Cliente> consulta = _context.Clientes
                .AsNoTracking()
                .Include(c => c.Empresa);

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var cliente = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            var clienteVM = new ClienteDeleteVM
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Empresa = cliente.Empresa.Nombre,
                Estado = cliente.Estado
            };

            return View(clienteVM);
        }

        // POST: Cliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin = await _userManager.IsInRoleAsync(usuario, "SuperAdmin");

            IQueryable<Cliente> consulta = _context.Clientes;

            if (!esSuperAdmin)
            {
                consulta = consulta.Where(c => c.EmpresaId == usuario.EmpresaId);
            }

            var cliente = await consulta.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            if (!cliente.Estado)
            {
                TempData["Error"] = "El cliente ya se encuentra inactivo.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                cliente.Estado = false;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cliente desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] = "Ocurrió un error al desactivar el cliente.";

                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task CargarEmpresas(ClienteCreateVM clienteVM)
        {
            clienteVM.Empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.Estado)
                .OrderBy(e => e.Nombre)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre,
                    Selected = e.Id == clienteVM.EmpresaId
                })
                .ToListAsync();
        }
        private async Task CargarEmpresas(ClienteEditVM clienteVM)
        {
            clienteVM.Empresas = await _context.Empresas
                .AsNoTracking()
                .Where(e => e.Estado || e.Id == clienteVM.EmpresaId)
                .OrderBy(e => e.Nombre)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Nombre,
                    Selected = e.Id == clienteVM.EmpresaId
                })
                .ToListAsync();
        }

    }
}
