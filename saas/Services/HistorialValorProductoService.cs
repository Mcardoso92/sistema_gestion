using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;
using saas.Models.Enums;

namespace saas.Services
{
    /// <summary>
    /// Centraliza el alta de eventos de costo y precio de venta para que todos
    /// los flujos conserven la misma información de auditoría.
    /// </summary>
    public class HistorialValorProductoService
    {
        private readonly SaasDbContext _context;

        public HistorialValorProductoService(SaasDbContext context)
        {
            _context = context;
        }

        public void Registrar(
            Producto producto,
            string usuarioId,
            TipoValorProducto tipoValor,
            decimal valorAnterior,
            decimal valorNuevo,
            DateTime fecha,
            OrigenCambioValorProducto origen,
            string? motivo = null,
            Compra? compra = null,
            Guid? operacionId = null,
            CambioValorProducto? cambioRevertido = null)
        {
            if (valorAnterior == valorNuevo)
            {
                return;
            }

            _context.CambiosValorProducto.Add(new CambioValorProducto
            {
                Producto = producto,
                EmpresaId = producto.EmpresaId,
                UsuarioId = usuarioId,
                Compra = compra,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                TipoValor = tipoValor,
                Fecha = fecha,
                Origen = origen,
                Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
                OperacionId = operacionId,
                CambioRevertido = cambioRevertido
            });
        }

        public async Task<ResultadoReversionValorProducto> RevertirAsync(
            int cambioId,
            int empresaId,
            string usuarioId,
            DateTime fecha)
        {
            CambioValorProducto? cambio = await _context.CambiosValorProducto
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == cambioId && c.EmpresaId == empresaId);

            if (cambio == null)
            {
                return ResultadoReversionValorProducto.Fallido("El cambio seleccionado no existe.");
            }

            if (cambio.Origen != OrigenCambioValorProducto.EdicionManual)
            {
                return ResultadoReversionValorProducto.Fallido(
                    "Solo pueden revertirse cambios realizados mediante una edición manual.");
            }

            bool yaFueRevertido = await _context.CambiosValorProducto
                .AnyAsync(c => c.CambioRevertidoId == cambio.Id);

            if (yaFueRevertido)
            {
                return ResultadoReversionValorProducto.Fallido("Este cambio ya fue revertido.");
            }

            bool existeCambioPosterior = await _context.CambiosValorProducto
                .AnyAsync(c =>
                    c.ProductoId == cambio.ProductoId &&
                    c.EmpresaId == cambio.EmpresaId &&
                    c.TipoValor == cambio.TipoValor &&
                    (c.Fecha > cambio.Fecha ||
                     (c.Fecha == cambio.Fecha && c.Id > cambio.Id)));

            decimal valorActual = cambio.TipoValor == TipoValorProducto.Costo
                ? cambio.Producto.PrecioCosto
                : cambio.Producto.PrecioVenta;

            if (existeCambioPosterior || valorActual != cambio.ValorNuevo)
            {
                return ResultadoReversionValorProducto.Fallido(
                    "No puede revertirse porque el valor tuvo modificaciones posteriores.");
            }

            string motivo = $"Reversión del cambio #{cambio.Id}.";

            Registrar(
                cambio.Producto,
                usuarioId,
                cambio.TipoValor,
                valorActual,
                cambio.ValorAnterior,
                fecha,
                OrigenCambioValorProducto.Reversion,
                motivo,
                cambioRevertido: cambio);

            if (cambio.TipoValor == TipoValorProducto.Costo)
            {
                cambio.Producto.PrecioCosto = cambio.ValorAnterior;
            }
            else
            {
                cambio.Producto.PrecioVenta = cambio.ValorAnterior;
            }

            await _context.SaveChangesAsync();

            return ResultadoReversionValorProducto.Correcto();
        }
    }

    public record ResultadoReversionValorProducto(bool Exito, string? Error)
    {
        public static ResultadoReversionValorProducto Correcto() => new(true, null);

        public static ResultadoReversionValorProducto Fallido(string error) => new(false, error);
    }
}
