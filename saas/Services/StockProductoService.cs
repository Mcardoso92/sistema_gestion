using saas.Models;
using saas.Models.Enums;

namespace saas.Services
{
    /// <summary>
    /// Centraliza las reglas que determinan si un producto modifica inventario.
    /// Los controladores deciden el motivo comercial; este servicio aplica el efecto físico.
    /// </summary>
    public class StockProductoService
    {
        public bool TieneDisponible(Producto producto, int cantidad)
        {
            return !producto.ControlaStock || producto.Stock >= cantidad;
        }

        public MovimientoStock? RegistrarEntrada(
            Producto producto,
            int cantidad,
            int empresaId,
            TipoMovimientoStock tipo,
            DateTime fecha,
            string usuarioId,
            string? motivo = null,
            int? ventaId = null,
            int? compraId = null,
            int? reintegroVentaId = null)
        {
            return RegistrarMovimiento(
                producto,
                cantidad,
                empresaId,
                tipo,
                fecha,
                usuarioId,
                esEntrada: true,
                motivo,
                ventaId,
                compraId,
                reintegroVentaId);
        }

        public MovimientoStock? RegistrarStockInicial(
            Producto producto,
            int empresaId,
            DateTime fecha,
            string usuarioId,
            string motivo = "Stock inicial")
        {
            if (!producto.ControlaStock || producto.Stock <= 0)
            {
                return null;
            }

            return new MovimientoStock
            {
                Producto = producto,
                EmpresaId = empresaId,
                Tipo = TipoMovimientoStock.StockInicial,
                Cantidad = producto.Stock,
                StockAnterior = 0,
                StockPosterior = producto.Stock,
                Motivo = motivo,
                Fecha = fecha,
                UsuarioId = usuarioId
            };
        }

        public MovimientoStock? RegistrarSalida(
            Producto producto,
            int cantidad,
            int empresaId,
            TipoMovimientoStock tipo,
            DateTime fecha,
            string usuarioId,
            string? motivo = null,
            int? ventaId = null,
            int? compraId = null,
            int? reintegroVentaId = null)
        {
            return RegistrarMovimiento(
                producto,
                cantidad,
                empresaId,
                tipo,
                fecha,
                usuarioId,
                esEntrada: false,
                motivo,
                ventaId,
                compraId,
                reintegroVentaId);
        }

        private static MovimientoStock? RegistrarMovimiento(
            Producto producto,
            int cantidad,
            int empresaId,
            TipoMovimientoStock tipo,
            DateTime fecha,
            string usuarioId,
            bool esEntrada,
            string? motivo,
            int? ventaId,
            int? compraId,
            int? reintegroVentaId)
        {
            if (!producto.ControlaStock)
            {
                return null;
            }

            int stockAnterior = producto.Stock;
            int stockPosterior = esEntrada
                ? checked(stockAnterior + cantidad)
                : stockAnterior - cantidad;

            producto.Stock = stockPosterior;

            return new MovimientoStock
            {
                ProductoId = producto.Id,
                EmpresaId = empresaId,
                Tipo = tipo,
                Cantidad = cantidad,
                StockAnterior = stockAnterior,
                StockPosterior = stockPosterior,
                Motivo = motivo,
                Fecha = fecha,
                UsuarioId = usuarioId,
                VentaId = ventaId,
                CompraId = compraId,
                ReintegroVentaId = reintegroVentaId
            };
        }
    }
}
