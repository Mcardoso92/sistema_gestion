using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models.Enums;
using saas.ViewModel.CuentaCorriente;

namespace saas.Services
{
    public class CuentaCorrienteService
    {
        private const int TamanioPagina = 20;
        private readonly SaasDbContext _context;
        private readonly VentaSaldoService _ventaSaldoService;
        private readonly CompraSaldoService _compraSaldoService;

        public CuentaCorrienteService(
            SaasDbContext context,
            VentaSaldoService ventaSaldoService,
            CompraSaldoService compraSaldoService)
        {
            _context = context;
            _ventaSaldoService = ventaSaldoService;
            _compraSaldoService = compraSaldoService;
        }

        public async Task<CuentaCorrienteVM?> ObtenerClienteAsync(
            int clienteId,
            int? empresaPermitidaId,
            string estado,
            int pagina)
        {
            var consultaCliente = _context.Clientes.AsNoTracking()
                .Where(c => c.Id == clienteId);

            if (empresaPermitidaId.HasValue)
            {
                consultaCliente = consultaCliente.Where(c => c.EmpresaId == empresaPermitidaId.Value);
            }

            var cliente = await consultaCliente
                .Select(c => new
                {
                    c.Id,
                    c.EmpresaId,
                    c.Nombre,
                    c.Apellido,
                    Empresa = c.Empresa.Nombre
                })
                .FirstOrDefaultAsync();

            if (cliente == null) return null;

            var movimientos = new List<CuentaCorrienteMovimientoVM>();

            movimientos.AddRange(await _context.Ventas.AsNoTracking()
                .Where(v => v.ClienteId == cliente.Id && v.EmpresaId == cliente.EmpresaId)
                .Select(v => new CuentaCorrienteMovimientoVM
                {
                    Fecha = v.Fecha,
                    Tipo = "Venta",
                    Referencia = "Venta #" + v.Id,
                    Importe = v.Total,
                    EfectoSaldo = v.Estado ? v.Total : 0,
                    Activo = v.Estado,
                    ControladorReferencia = "Venta",
                    IdReferencia = v.Id,
                    Orden = 0,
                    IdOperacion = v.Id
                }).ToListAsync());

            movimientos.AddRange(await _context.CobrosVenta.AsNoTracking()
                .Where(c => c.Venta.ClienteId == cliente.Id && c.EmpresaId == cliente.EmpresaId)
                .Select(c => new CuentaCorrienteMovimientoVM
                {
                    Fecha = c.Fecha,
                    Tipo = "Cobro",
                    Referencia = "Cobro #" + c.Id + " · Venta #" + c.VentaId,
                    Importe = c.Importe,
                    EfectoSaldo = c.Estado == EstadoCobro.Activo && c.Venta.Estado ? -c.Importe : 0,
                    Activo = c.Estado == EstadoCobro.Activo && c.Venta.Estado,
                    ControladorReferencia = "Venta",
                    IdReferencia = c.VentaId,
                    Orden = 1,
                    IdOperacion = c.Id
                }).ToListAsync());

            movimientos.AddRange(await _context.ReintegrosVenta.AsNoTracking()
                .Where(r => r.Venta.ClienteId == cliente.Id && r.EmpresaId == cliente.EmpresaId)
                .Select(r => new CuentaCorrienteMovimientoVM
                {
                    Fecha = r.Fecha,
                    Tipo = "Reintegro",
                    Referencia = "Reintegro #" + r.Id + " · Venta #" + r.VentaId,
                    Importe = r.Importe,
                    EfectoSaldo = 0,
                    Activo = r.Estado == EstadoReintegro.Activo && r.Venta.Estado,
                    ControladorReferencia = "Venta",
                    IdReferencia = r.VentaId,
                    Orden = 2,
                    IdOperacion = r.Id
                }).ToListAsync());

            var modelo = new CuentaCorrienteVM
            {
                TitularId = cliente.Id,
                Titular = string.IsNullOrWhiteSpace(cliente.Apellido)
                    ? cliente.Nombre
                    : $"{cliente.Nombre} {cliente.Apellido}",
                Empresa = cliente.Empresa,
                EsCliente = true,
                SaldoPendiente = await _ventaSaldoService.ObtenerSaldoPendienteCliente(cliente.Id, cliente.EmpresaId)
            };

            PrepararMovimientos(modelo, movimientos, estado, pagina);
            return modelo;
        }

        public async Task<CuentaCorrienteVM?> ObtenerProveedorAsync(
            int proveedorId,
            int? empresaPermitidaId,
            string estado,
            int pagina)
        {
            var consultaProveedor = _context.Proveedores.AsNoTracking()
                .Where(p => p.Id == proveedorId);

            if (empresaPermitidaId.HasValue)
            {
                consultaProveedor = consultaProveedor.Where(p => p.EmpresaId == empresaPermitidaId.Value);
            }

            var proveedor = await consultaProveedor
                .Select(p => new
                {
                    p.Id,
                    p.EmpresaId,
                    p.RazonSocial,
                    Empresa = p.Empresa.Nombre
                })
                .FirstOrDefaultAsync();

            if (proveedor == null) return null;

            var movimientos = new List<CuentaCorrienteMovimientoVM>();

            movimientos.AddRange(await _context.Compras.AsNoTracking()
                .Where(c => c.ProveedorId == proveedor.Id && c.EmpresaId == proveedor.EmpresaId)
                .Select(c => new CuentaCorrienteMovimientoVM
                {
                    Fecha = c.Fecha,
                    Tipo = "Compra",
                    Referencia = "Compra #" + c.Id,
                    Importe = c.Total,
                    EfectoSaldo = c.Estado ? c.Total : 0,
                    Activo = c.Estado,
                    ControladorReferencia = "Compra",
                    IdReferencia = c.Id,
                    Orden = 0,
                    IdOperacion = c.Id
                }).ToListAsync());

            movimientos.AddRange(await _context.DevolucionesCompra.AsNoTracking()
                .Where(d => d.Compra.ProveedorId == proveedor.Id && d.EmpresaId == proveedor.EmpresaId)
                .Select(d => new CuentaCorrienteMovimientoVM
                {
                    Fecha = d.Fecha,
                    Tipo = "Devolución",
                    Referencia = "Devolución #" + d.Id + " · Compra #" + d.CompraId,
                    Importe = d.Total,
                    EfectoSaldo = d.Estado && d.Compra.Estado ? -d.Total : 0,
                    Activo = d.Estado && d.Compra.Estado,
                    ControladorReferencia = "Compra",
                    IdReferencia = d.CompraId,
                    Orden = 1,
                    IdOperacion = d.Id
                }).ToListAsync());

            movimientos.AddRange(await _context.PagosProveedor.AsNoTracking()
                .Where(p => p.Compra.ProveedorId == proveedor.Id && p.EmpresaId == proveedor.EmpresaId)
                .Select(p => new CuentaCorrienteMovimientoVM
                {
                    Fecha = p.Fecha,
                    Tipo = "Pago",
                    Referencia = "Pago #" + p.Id + " · Compra #" + p.CompraId,
                    Importe = p.Importe,
                    EfectoSaldo = p.Estado == EstadoPago.Activo && p.Compra.Estado ? -p.Importe : 0,
                    Activo = p.Estado == EstadoPago.Activo && p.Compra.Estado,
                    ControladorReferencia = "Compra",
                    IdReferencia = p.CompraId,
                    Orden = 2,
                    IdOperacion = p.Id
                }).ToListAsync());

            movimientos.AddRange(await _context.ReintegrosProveedor.AsNoTracking()
                .Where(r => r.Compra.ProveedorId == proveedor.Id && r.EmpresaId == proveedor.EmpresaId)
                .Select(r => new CuentaCorrienteMovimientoVM
                {
                    Fecha = r.Fecha,
                    Tipo = "Reintegro",
                    Referencia = "Reintegro #" + r.Id + " · Compra #" + r.CompraId,
                    Importe = r.Importe,
                    EfectoSaldo = r.Estado == EstadoReintegro.Activo && r.Compra.Estado ? r.Importe : 0,
                    Activo = r.Estado == EstadoReintegro.Activo && r.Compra.Estado,
                    ControladorReferencia = "Compra",
                    IdReferencia = r.CompraId,
                    Orden = 3,
                    IdOperacion = r.Id
                }).ToListAsync());

            PosicionProveedor posicion = await _compraSaldoService
                .ObtenerPosicionProveedor(proveedor.Id, proveedor.EmpresaId);

            var modelo = new CuentaCorrienteVM
            {
                TitularId = proveedor.Id,
                Titular = proveedor.RazonSocial,
                Empresa = proveedor.Empresa,
                EsCliente = false,
                SaldoPendiente = posicion.SaldoPendiente,
                SaldoARecuperar = posicion.SaldoARecuperar
            };

            PrepararMovimientos(modelo, movimientos, estado, pagina);
            return modelo;
        }

        private static void PrepararMovimientos(
            CuentaCorrienteVM modelo,
            List<CuentaCorrienteMovimientoVM> movimientos,
            string estado,
            int pagina)
        {
            var ordenados = movimientos
                .OrderBy(m => m.Fecha)
                .ThenBy(m => m.Orden)
                .ThenBy(m => m.IdOperacion)
                .ToList();

            decimal saldo = 0;
            foreach (CuentaCorrienteMovimientoVM movimiento in ordenados)
            {
                saldo += movimiento.EfectoSaldo;
                movimiento.SaldoResultante = saldo;
            }

            modelo.EstadoFiltro = estado?.ToLowerInvariant() switch
            {
                "activos" => "activos",
                "anulados" => "anulados",
                _ => "todos"
            };

            IEnumerable<CuentaCorrienteMovimientoVM> filtrados = modelo.EstadoFiltro switch
            {
                "activos" => ordenados.Where(m => m.Activo),
                "anulados" => ordenados.Where(m => !m.Activo),
                _ => ordenados
            };

            var listaFiltrada = filtrados.ToList();
            modelo.TotalRegistros = listaFiltrada.Count;
            modelo.TotalPaginas = (int)Math.Ceiling(modelo.TotalRegistros / (double)TamanioPagina);
            modelo.PaginaActual = Math.Max(1, pagina);
            if (modelo.TotalPaginas > 0 && modelo.PaginaActual > modelo.TotalPaginas)
            {
                modelo.PaginaActual = modelo.TotalPaginas;
            }

            modelo.Movimientos = listaFiltrada
                .Skip((modelo.PaginaActual - 1) * TamanioPagina)
                .Take(TamanioPagina)
                .ToList();
        }
    }
}
