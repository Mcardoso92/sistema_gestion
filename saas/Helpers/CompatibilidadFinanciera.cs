using saas.Models.Enums;

namespace saas.Helpers
{
    public static class CompatibilidadFinanciera
    {
        public static bool EsCompatible(
            TipoCaja tipoCaja,
            TipoMedioPago tipoMedioPago)
        {
            return tipoCaja switch
            {
                TipoCaja.Efectivo =>
                    tipoMedioPago == TipoMedioPago.Efectivo,

                TipoCaja.Banco =>
                    tipoMedioPago == TipoMedioPago.Transferencia ||
                    tipoMedioPago == TipoMedioPago.TarjetaDebito ||
                    tipoMedioPago == TipoMedioPago.TarjetaCredito ||
                    tipoMedioPago == TipoMedioPago.Cheque,

                TipoCaja.BilleteraVirtual =>
                    tipoMedioPago == TipoMedioPago.Transferencia ||
                    tipoMedioPago == TipoMedioPago.QR,

                _ =>
                    tipoMedioPago == TipoMedioPago.Otro
            };
        }
    }
}