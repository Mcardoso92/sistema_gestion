namespace saas.Models.Enums
{
    public enum TipoMovimientoStock
    {
        [System.ComponentModel.DataAnnotations.Display(Name = "Stock inicial")]
        StockInicial = 1,
        [System.ComponentModel.DataAnnotations.Display(Name = "Ajuste de entrada")]
        AjusteEntrada = 2,
        [System.ComponentModel.DataAnnotations.Display(Name = "Ajuste de salida")]
        AjusteSalida = 3,
        Venta = 4,
        [System.ComponentModel.DataAnnotations.Display(Name = "Anulación de venta")]
        AnulacionVenta = 5,
        Compra = 6,
        [System.ComponentModel.DataAnnotations.Display(Name = "Anulación de compra")]
        AnulacionCompra = 7,
        [System.ComponentModel.DataAnnotations.Display(Name = "Reintegro de venta")]
        ReintegroVenta = 8,
        [System.ComponentModel.DataAnnotations.Display(Name = "Anulación de reintegro de venta")]
        AnulacionReintegroVenta = 9,
        [System.ComponentModel.DataAnnotations.Display(Name = "Devolución de compra")]
        DevolucionCompra = 10,
        [System.ComponentModel.DataAnnotations.Display(Name = "Anulación de devolución de compra")]
        AnulacionDevolucionCompra = 11
    }
}
