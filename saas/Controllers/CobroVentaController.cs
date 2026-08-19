using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel;
using System.Data;

namespace saas.Controllers
{
    [Authorize(Roles = "SuperAdmin,AdminEmpresa")]
    public class CobroVentaController : Controller
    {
        private readonly SaasDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public CobroVentaController(
            SaasDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CobroVenta/Registrar/5
        [HttpGet]
        public async Task<IActionResult> Registrar(int ventaId)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<Venta> consulta =
                _context.Ventas
                    .AsNoTracking();

            if (!esSuperAdmin)
            {
                consulta =
                    consulta.Where(v =>
                        v.EmpresaId == usuario.EmpresaId);
            }

            var venta =
                await consulta
                    .FirstOrDefaultAsync(v =>
                        v.Id == ventaId);

            if (venta == null)
            {
                return NotFound();
            }

            if (!venta.Estado)
            {
                TempData["Error"] =
                    "No se pueden registrar cobros sobre una venta anulada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            decimal totalCobrado =
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.VentaId == venta.Id &&
                        c.Estado == EstadoCobro.Activo)
                    .SumAsync(c =>
                        (decimal?)c.Importe)
                    ?? 0;

            decimal saldoPendiente =
                Math.Max(
                    0,
                    venta.Total - totalCobrado);

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La venta no tiene saldo pendiente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            var vm =
                new RegistrarCobroVentaVM
                {
                    VentaId =
                        venta.Id,

                    SaldoPendiente =
                        saldoPendiente,

                    Importe =
                        saldoPendiente
                };

            await CargarOpciones(
                vm,
                venta.EmpresaId);

            return View(vm);
        }
        // POST: CobroVenta/Registrar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarCobroVentaVM vm)
        {
            var usuario =
                await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return Challenge();
            }

            bool esSuperAdmin =
                await _userManager.IsInRoleAsync(
                    usuario,
                    "SuperAdmin");

            IQueryable<Venta> consultaVenta =
                _context.Ventas;

            if (!esSuperAdmin)
            {
                consultaVenta =
                    consultaVenta.Where(v =>
                        v.EmpresaId == usuario.EmpresaId);
            }

            var venta =
                await consultaVenta
                    .FirstOrDefaultAsync(v =>
                        v.Id == vm.VentaId);

            if (venta == null)
            {
                return NotFound();
            }

            if (!venta.Estado)
            {
                TempData["Error"] =
                    "No se pueden registrar cobros sobre una venta anulada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            decimal totalCobrado =
                await _context.CobrosVenta
                    .AsNoTracking()
                    .Where(c =>
                        c.VentaId == venta.Id &&
                        c.Estado == EstadoCobro.Activo)
                    .SumAsync(c =>
                        (decimal?)c.Importe)
                ?? 0;

            decimal saldoPendiente =
                Math.Max(
                    0,
                    venta.Total - totalCobrado);

            vm.SaldoPendiente =
                saldoPendiente;

            if (saldoPendiente <= 0)
            {
                TempData["Error"] =
                    "La venta ya se encuentra completamente cobrada.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }

            if (vm.Importe > saldoPendiente)
            {
                ModelState.AddModelError(
                    nameof(vm.Importe),
                    "El importe no puede superar el saldo pendiente.");
            }

            var caja =
                await _context.Cajas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == vm.CajaId &&
                        c.EmpresaId == venta.EmpresaId &&
                        c.Estado);

            if (caja == null)
            {
                ModelState.AddModelError(
                    nameof(vm.CajaId),
                    "La caja seleccionada no es válida.");
            }

            bool medioPagoValido = false;

            if (caja != null)
            {
                medioPagoValido =
                    await _context.CajaMediosPago
                        .AsNoTracking()
                        .AnyAsync(cm =>
                            cm.CajaId == vm.CajaId &&
                            cm.MedioPagoId == vm.MedioPagoId &&
                            cm.Caja.EmpresaId == venta.EmpresaId &&
                            cm.Caja.Estado &&
                            cm.MedioPago.EmpresaId == venta.EmpresaId &&
                            cm.MedioPago.Estado);
            }

            if (!medioPagoValido)
            {
                ModelState.AddModelError(
                    nameof(vm.MedioPagoId),
                    "El medio de pago no es válido para la caja seleccionada.");
            }

            var turnoOperativo =
                await _context.TurnosCaja
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t =>
                        t.EmpresaId == venta.EmpresaId &&
                        t.UsuarioAperturaId == usuario.Id &&
                        t.Estado == EstadoTurnoCaja.Abierto);

            int? turnoMovimientoCajaId = null;

            if (caja != null &&
                caja.PermiteTurnos)
            {
                if (turnoOperativo == null ||
                    turnoOperativo.CajaId != caja.Id)
                {
                    ModelState.AddModelError(
                        nameof(vm.CajaId),
                        $"Debe tener un turno abierto propio para operar la caja \"{caja.Nombre}\".");
                }
                else
                {
                    turnoMovimientoCajaId =
                        turnoOperativo.Id;
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarOpciones(
                    vm,
                    venta.EmpresaId);

                return View(vm);
            }

            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);

            try
            {
                var cobro =
                    new CobroVenta
                    {
                        VentaId =
                            venta.Id,

                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        MedioPagoId =
                            vm.MedioPagoId,

                        // Turno operativo del usuario,
                        // aunque el movimiento no afecte arqueo.
                        TurnoCajaId =
                            turnoOperativo?.Id,

                        UsuarioId =
                            usuario.Id,

                        Fecha =
                            DateTime.Now,

                        Importe =
                            vm.Importe,

                        Estado =
                            EstadoCobro.Activo,

                        FechaAnulacion =
                            null,

                        UsuarioAnulacionId =
                            null,

                        MotivoAnulacion =
                            null
                    };

                _context.CobrosVenta.Add(
                    cobro);

                await _context.SaveChangesAsync();

                var movimiento =
                    new MovimientoCaja
                    {
                        EmpresaId =
                            venta.EmpresaId,

                        CajaId =
                            vm.CajaId,

                        Tipo =
                            TipoMovimientoCaja.CobroVenta,

                        Direccion =
                            DireccionMovimientoCaja.Ingreso,

                        Importe =
                            vm.Importe,

                        Fecha =
                            cobro.Fecha,

                        UsuarioId =
                            usuario.Id,

                        MedioPagoId =
                            vm.MedioPagoId,

                        // Solo impacta arqueo si esa caja
                        // trabaja con el turno.
                        TurnoCajaId =
                            turnoMovimientoCajaId,

                        CategoriaGastoId =
                            null,

                        Concepto =
                            $"Cobro de venta #{venta.Id}",

                        Observaciones =
                            null,

                        CobroVentaId =
                            cobro.Id
                    };

                _context.MovimientosCaja.Add(
                    movimiento);

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                TempData["Success"] =
                    vm.Importe == saldoPendiente
                        ? "La venta quedó completamente cobrada."
                        : "Cobro registrado correctamente.";

                return RedirectToAction(
                    "Details",
                    "Venta",
                    new { id = venta.Id });
            }
            catch
            {
                await transaccion.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Ocurrió un error al registrar el cobro.");

                await CargarOpciones(
                    vm,
                    venta.EmpresaId);

                return View(vm);
            }
        }
        private async Task CargarOpciones(
    RegistrarCobroVentaVM vm,
    int empresaId)
        {
            vm.CajasDisponibles =
                await _context.Cajas
                    .AsNoTracking()
                    .Where(c =>
                        c.EmpresaId == empresaId &&
                        c.Estado)
                    .OrderBy(c =>
                        c.Nombre)
                    .Select(c =>
                        new CajaOpcionSimpleVM
                        {
                            Id = c.Id,
                            Nombre = c.Nombre
                        })
                    .ToListAsync();

            vm.MediosPagoDisponibles =
                await _context.MediosPago
                    .AsNoTracking()
                    .Where(m =>
                        m.EmpresaId == empresaId &&
                        m.Estado)
                    .OrderBy(m =>
                        m.Nombre)
                    .Select(m =>
                        new MedioPagoOpcionSimpleVM
                        {
                            Id = m.Id,
                            Nombre = m.Nombre,
                            Tipo = m.Tipo
                        })
                    .ToListAsync();
        }
    }
}