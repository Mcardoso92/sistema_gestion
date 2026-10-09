using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using saas.Data;
using saas.Models;
using saas.Models.Enums;
using saas.ViewModel.ProductoActualizacionMasiva;
using System.Data;

namespace saas.Services
{
    public class ProductoActualizacionMasivaService : IProductoActualizacionMasivaService
    {
        private const decimal ValorMaximo = 999999999.99m;
        private static readonly TimeSpan DuracionVistaPrevia = TimeSpan.FromMinutes(30);
        private readonly SaasDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly HistorialValorProductoService _historial;
        private readonly IFechaHoraService _fechaHora;

        public ProductoActualizacionMasivaService(
            SaasDbContext context,
            IMemoryCache cache,
            HistorialValorProductoService historial,
            IFechaHoraService fechaHora)
        {
            _context = context;
            _cache = cache;
            _historial = historial;
            _fechaHora = fechaHora;
        }

        public async Task<ProductoActualizacionMasivaVistaPreviaVM> PrepararVistaPreviaAsync(
            ProductoActualizacionMasivaVM modelo,
            int empresaId,
            string usuarioId)
        {
            if (modelo.ValorAjuste == 0)
            {
                throw new InvalidOperationException("El ajuste debe ser distinto de cero.");
            }

            IQueryable<Producto> consulta = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.EmpresaId == empresaId && p.Estado);

            if (modelo.CategoriaId.HasValue)
            {
                bool categoriaValida = await _context.Categorias.AsNoTracking().AnyAsync(c =>
                    c.Id == modelo.CategoriaId.Value &&
                    c.EmpresaId == empresaId &&
                    c.Estado);

                if (!categoriaValida)
                {
                    throw new InvalidOperationException("La categoría seleccionada no es válida para la empresa actual.");
                }

                consulta = consulta.Where(p => p.CategoriaId == modelo.CategoriaId.Value);
            }

            string? busqueda = string.IsNullOrWhiteSpace(modelo.Busqueda)
                ? null
                : modelo.Busqueda.Trim();

            if (busqueda != null)
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    (p.CodigoBarra != null && p.CodigoBarra.Contains(busqueda)));
            }

            List<Producto> productos = await consulta.OrderBy(p => p.Nombre).ToListAsync();
            var vistaPrevia = new ProductoActualizacionMasivaVistaPreviaVM
            {
                Token = Guid.NewGuid().ToString("N"),
                EmpresaId = empresaId,
                CategoriaId = modelo.CategoriaId,
                Busqueda = busqueda,
                Campo = modelo.Campo,
                TipoAjuste = modelo.TipoAjuste,
                ValorAjuste = modelo.ValorAjuste,
                Motivo = modelo.Motivo.Trim()
            };

            foreach (Producto producto in productos)
            {
                decimal costoNuevo = IncluyeCosto(modelo.Campo)
                    ? CalcularValor(producto.PrecioCosto, modelo.TipoAjuste, modelo.ValorAjuste)
                    : producto.PrecioCosto;
                decimal ventaNueva = IncluyeVenta(modelo.Campo)
                    ? CalcularValor(producto.PrecioVenta, modelo.TipoAjuste, modelo.ValorAjuste)
                    : producto.PrecioVenta;

                ValidarResultado(costoNuevo, "costo", producto.Nombre);
                ValidarResultado(ventaNueva, "precio de venta", producto.Nombre);

                if (costoNuevo == producto.PrecioCosto && ventaNueva == producto.PrecioVenta)
                {
                    continue;
                }

                vistaPrevia.Productos.Add(new ProductoActualizacionMasivaFilaVM
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Categoria = producto.Categoria.Nombre,
                    CostoAnterior = producto.PrecioCosto,
                    CostoNuevo = costoNuevo,
                    VentaAnterior = producto.PrecioVenta,
                    VentaNueva = ventaNueva
                });
            }

            if (vistaPrevia.TotalProductos == 0)
            {
                throw new InvalidOperationException("No hay productos activos que resulten modificados con los criterios indicados.");
            }

            _cache.Set(
                ClaveCache(vistaPrevia.Token),
                new ActualizacionTemporal(empresaId, usuarioId, vistaPrevia),
                DuracionVistaPrevia);

            return vistaPrevia;
        }

        public bool TryObtenerVistaPrevia(
            string token,
            int empresaId,
            string usuarioId,
            out ProductoActualizacionMasivaVistaPreviaVM? vistaPrevia)
        {
            bool encontrada = _cache.TryGetValue(ClaveCache(token), out ActualizacionTemporal? temporal);
            vistaPrevia = encontrada && temporal!.EmpresaId == empresaId && temporal.UsuarioId == usuarioId
                ? temporal.VistaPrevia
                : null;
            return vistaPrevia != null;
        }

        public async Task<int> AplicarAsync(string token, int empresaId, string usuarioId)
        {
            if (!TryObtenerVistaPrevia(token, empresaId, usuarioId, out ProductoActualizacionMasivaVistaPreviaVM? vistaPrevia))
            {
                throw new InvalidOperationException("La vista previa venció o no pertenece al usuario actual.");
            }

            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                int[] ids = vistaPrevia!.Productos.Select(p => p.ProductoId).ToArray();
                List<Producto> productos = await _context.Productos
                    .Where(p => ids.Contains(p.Id) && p.EmpresaId == empresaId && p.Estado)
                    .ToListAsync();

                if (productos.Count != ids.Length)
                {
                    throw new InvalidOperationException("Uno o más productos dejaron de estar disponibles. Genere nuevamente la vista previa.");
                }

                Dictionary<int, ProductoActualizacionMasivaFilaVM> filas =
                    vistaPrevia.Productos.ToDictionary(p => p.ProductoId);

                bool cambioDesdeVistaPrevia = productos.Any(p =>
                    p.PrecioCosto != filas[p.Id].CostoAnterior ||
                    p.PrecioVenta != filas[p.Id].VentaAnterior);

                if (cambioDesdeVistaPrevia)
                {
                    throw new InvalidOperationException("Uno o más precios cambiaron desde la vista previa. Revise nuevamente antes de confirmar.");
                }

                DateTime fecha = _fechaHora.UtcAhora;
                Guid operacionId = Guid.NewGuid();

                foreach (Producto producto in productos)
                {
                    ProductoActualizacionMasivaFilaVM fila = filas[producto.Id];

                    if (fila.ModificaCosto)
                    {
                        _historial.Registrar(
                            producto,
                            usuarioId,
                            TipoValorProducto.Costo,
                            fila.CostoAnterior,
                            fila.CostoNuevo,
                            fecha,
                            OrigenCambioValorProducto.ActualizacionMasiva,
                            vistaPrevia.Motivo,
                            operacionId: operacionId);
                        producto.PrecioCosto = fila.CostoNuevo;
                    }

                    if (fila.ModificaVenta)
                    {
                        _historial.Registrar(
                            producto,
                            usuarioId,
                            TipoValorProducto.PrecioVenta,
                            fila.VentaAnterior,
                            fila.VentaNueva,
                            fecha,
                            OrigenCambioValorProducto.ActualizacionMasiva,
                            vistaPrevia.Motivo,
                            operacionId: operacionId);
                        producto.PrecioVenta = fila.VentaNueva;
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                _cache.Remove(ClaveCache(token));
                return productos.Count;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        private static decimal CalcularValor(decimal valorActual, TipoAjustePrecioProducto tipo, decimal ajuste)
        {
            decimal resultado = tipo == TipoAjustePrecioProducto.Porcentaje
                ? valorActual * (1 + ajuste / 100m)
                : valorActual + ajuste;

            return decimal.Round(resultado, 2, MidpointRounding.AwayFromZero);
        }

        private static void ValidarResultado(decimal valor, string campo, string producto)
        {
            if (valor < 0 || valor > ValorMaximo)
            {
                throw new InvalidOperationException(
                    $"El {campo} resultante de '{producto}' queda fuera del rango permitido.");
            }
        }

        private static bool IncluyeCosto(CampoAjustePrecioProducto campo) =>
            campo is CampoAjustePrecioProducto.PrecioCosto or CampoAjustePrecioProducto.Ambos;

        private static bool IncluyeVenta(CampoAjustePrecioProducto campo) =>
            campo is CampoAjustePrecioProducto.PrecioVenta or CampoAjustePrecioProducto.Ambos;

        private static string ClaveCache(string token) => $"actualizacion-precios:{token}";

        private sealed record ActualizacionTemporal(
            int EmpresaId,
            string UsuarioId,
            ProductoActualizacionMasivaVistaPreviaVM VistaPrevia);
    }
}
