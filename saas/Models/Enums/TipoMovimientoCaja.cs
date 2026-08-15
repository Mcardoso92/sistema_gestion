namespace saas.Models.Enums
{
    public enum TipoMovimientoCaja
    {
        CobroVenta = 1,
        PagoProveedor = 2,

        ReintegroVenta = 3,
        ReintegroProveedor = 4,

        IngresoManual = 5,
        EgresoManual = 6,

        TransferenciaEntrada = 7,
        TransferenciaSalida = 8,

        AjusteSobranteCaja = 9,
        AjusteFaltanteCaja = 10,

        ReversionCobroVenta = 11,
        ReversionPagoProveedor = 12,

        ReversionReintegroVenta = 13,
        ReversionReintegroProveedor = 14,

        ReversionIngresoManual = 15,
        ReversionEgresoManual = 16,

        ReversionAjusteSobranteCaja = 17,
        ReversionAjusteFaltanteCaja = 18,

        ReversionTransferenciaEntrada = 19,
        ReversionTransferenciaSalida = 20
    }
}