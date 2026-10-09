using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using System.Data;

namespace saas.Services
{
    /// <summary>
    /// Centraliza la sesión de conteo y su conciliación para evitar ajustes
    /// parciales o sobrescrituras de stock sin movimientos trazables.
    /// </summary>
    public class InventarioFisicoService
    {
        private readonly SaasDbContext _context;
        private readonly StockProductoService _stockProductoService;
        private readonly IFechaHoraService _fechaHora;

        public InventarioFisicoService(
            SaasDbContext context,
            StockProductoService stockProductoService,
            IFechaHoraService fechaHora)
        {
            _context = context;
            _stockProductoService = stockProductoService;
            _fechaHora = fechaHora;
        }

        public async Task<InventarioFisico> CrearAsync(
            int empresaId,
            string usuarioId,
            string motivo,
            IEnumerable<int> productosIds)
        {
            int[] ids = productosIds.Distinct().ToArray();

            if (ids.Length == 0)
            {
                throw new InvalidOperationException("Seleccione al menos un producto para iniciar el inventario.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new InvalidOperationException("El motivo del inventario es obligatorio.");
            }

            if (motivo.Trim().Length > 250)
            {
                throw new InvalidOperationException("El motivo no puede superar los 250 caracteres.");
            }

            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                bool existeAbierto = await _context.InventariosFisicos.AnyAsync(i =>
                    i.EmpresaId == empresaId &&
                    i.Estado == EstadoInventarioFisico.Abierto);

                if (existeAbierto)
                {
                    throw new InvalidOperationException("La empresa ya tiene un inventario físico abierto.");
                }

                List<Producto> productos = await _context.Productos
                    .Where(p =>
                        ids.Contains(p.Id) &&
                        p.EmpresaId == empresaId &&
                        p.Estado &&
                        p.ControlaStock)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                if (productos.Count != ids.Length)
                {
                    throw new InvalidOperationException(
                        "Uno o más productos no pertenecen a la empresa, están inactivos o no controlan stock.");
                }

                var inventario = new InventarioFisico
                {
                    EmpresaId = empresaId,
                    UsuarioInicioId = usuarioId,
                    FechaInicio = _fechaHora.UtcAhora,
                    Estado = EstadoInventarioFisico.Abierto,
                    Motivo = motivo.Trim(),
                    Detalles = productos.Select(p => new DetalleInventarioFisico
                    {
                        ProductoId = p.Id,
                        StockTeorico = p.Stock
                    }).ToList()
                };

                _context.InventariosFisicos.Add(inventario);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return inventario;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task GuardarConteoAsync(
            int inventarioId,
            int empresaId,
            IReadOnlyDictionary<int, int?> cantidades)
        {
            InventarioFisico inventario = await ObtenerAbiertoAsync(inventarioId, empresaId);

            foreach (DetalleInventarioFisico detalle in inventario.Detalles)
            {
                if (!cantidades.TryGetValue(detalle.Id, out int? cantidad))
                {
                    continue;
                }

                if (cantidad < 0)
                {
                    throw new InvalidOperationException("Las cantidades contadas no pueden ser negativas.");
                }

                detalle.StockContado = cantidad;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> ConfirmarAsync(
            int inventarioId,
            int empresaId,
            string usuarioId)
        {
            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                InventarioFisico inventario = await ObtenerAbiertoAsync(inventarioId, empresaId);

                if (inventario.Detalles.Any(d => !d.StockContado.HasValue))
                {
                    throw new InvalidOperationException("Debe completar el conteo de todos los productos antes de confirmar.");
                }

                int[] productosIds = inventario.Detalles.Select(d => d.ProductoId).ToArray();
                bool huboMovimientosPosteriores = await _context.MovimientosStock
                    .AsNoTracking()
                    .AnyAsync(m =>
                        m.EmpresaId == empresaId &&
                        productosIds.Contains(m.ProductoId) &&
                        m.Fecha > inventario.FechaInicio);

                bool cambioStockSinMovimiento = inventario.Detalles.Any(d =>
                    d.Producto.Stock != d.StockTeorico);

                if (huboMovimientosPosteriores || cambioStockSinMovimiento)
                {
                    throw new InvalidOperationException(
                        "El stock cambió mientras el inventario estaba abierto. Cancele la sesión e inicie un nuevo conteo.");
                }

                DateTime fecha = _fechaHora.UtcAhora;
                int ajustes = 0;

                foreach (DetalleInventarioFisico detalle in inventario.Detalles)
                {
                    int diferencia = detalle.StockContado!.Value - detalle.StockTeorico;
                    if (diferencia == 0)
                    {
                        continue;
                    }

                    string motivoMovimiento = $"Inventario físico #{inventario.Id}: {inventario.Motivo}";
                    MovimientoStock? movimiento = diferencia > 0
                        ? _stockProductoService.RegistrarEntrada(
                            detalle.Producto,
                            diferencia,
                            empresaId,
                            TipoMovimientoStock.AjusteEntrada,
                            fecha,
                            usuarioId,
                            motivoMovimiento)
                        : _stockProductoService.RegistrarSalida(
                            detalle.Producto,
                            Math.Abs(diferencia),
                            empresaId,
                            TipoMovimientoStock.AjusteSalida,
                            fecha,
                            usuarioId,
                            motivoMovimiento);

                    if (movimiento != null)
                    {
                        movimiento.InventarioFisico = inventario;
                        _context.MovimientosStock.Add(movimiento);
                        ajustes++;
                    }
                }

                inventario.Estado = EstadoInventarioFisico.Confirmado;
                inventario.FechaConfirmacion = fecha;
                inventario.UsuarioConfirmacionId = usuarioId;

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return ajustes;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task CancelarAsync(int inventarioId, int empresaId)
        {
            InventarioFisico inventario = await ObtenerAbiertoAsync(inventarioId, empresaId);
            inventario.Estado = EstadoInventarioFisico.Cancelado;
            await _context.SaveChangesAsync();
        }

        private async Task<InventarioFisico> ObtenerAbiertoAsync(int inventarioId, int empresaId)
        {
            InventarioFisico? inventario = await _context.InventariosFisicos
                .Include(i => i.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(i =>
                    i.Id == inventarioId &&
                    i.EmpresaId == empresaId &&
                    i.Estado == EstadoInventarioFisico.Abierto);

            return inventario ?? throw new InvalidOperationException(
                "El inventario no existe, no pertenece a la empresa o ya fue cerrado.");
        }
    }
}
