namespace saas.Models.Enums
{
    public enum TipoMovimientoCaja
    {
        [System.ComponentModel.DataAnnotations.Display(Name = "Cobro de venta")]
        CobroVenta = 1,
        [System.ComponentModel.DataAnnotations.Display(Name = "Pago a proveedor")]
        PagoProveedor = 2,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reintegro de venta")]
        ReintegroVenta = 3,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reintegro de proveedor")]
        ReintegroProveedor = 4,

        [System.ComponentModel.DataAnnotations.Display(Name = "Ingreso manual")]
        IngresoManual = 5,
        [System.ComponentModel.DataAnnotations.Display(Name = "Egreso manual")]
        EgresoManual = 6,

        [System.ComponentModel.DataAnnotations.Display(Name = "Transferencia de entrada")]
        TransferenciaEntrada = 7,
        [System.ComponentModel.DataAnnotations.Display(Name = "Transferencia de salida")]
        TransferenciaSalida = 8,

        [System.ComponentModel.DataAnnotations.Display(Name = "Ajuste por sobrante de caja")]
        AjusteSobranteCaja = 9,
        [System.ComponentModel.DataAnnotations.Display(Name = "Ajuste por faltante de caja")]
        AjusteFaltanteCaja = 10,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de cobro de venta")]
        ReversionCobroVenta = 11,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de pago a proveedor")]
        ReversionPagoProveedor = 12,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de reintegro de venta")]
        ReversionReintegroVenta = 13,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de reintegro de proveedor")]
        ReversionReintegroProveedor = 14,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de ingreso manual")]
        ReversionIngresoManual = 15,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de egreso manual")]
        ReversionEgresoManual = 16,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de ajuste por sobrante")]
        ReversionAjusteSobranteCaja = 17,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de ajuste por faltante")]
        ReversionAjusteFaltanteCaja = 18,

        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de transferencia de entrada")]
        ReversionTransferenciaEntrada = 19,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reversión de transferencia de salida")]
        ReversionTransferenciaSalida = 20
    }
}
