namespace saas.Models.Enums
{
    public enum TipoMedioPago
    {
        Efectivo = 1,
        Transferencia = 2,
        [System.ComponentModel.DataAnnotations.Display(Name = "Tarjeta de débito")]
        TarjetaDebito = 3,
        [System.ComponentModel.DataAnnotations.Display(Name = "Tarjeta de crédito")]
        TarjetaCredito = 4,
        QR = 5,
        Cheque = 6,
        Otro = 7
    }
}
