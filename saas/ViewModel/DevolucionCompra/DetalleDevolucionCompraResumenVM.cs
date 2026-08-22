namespace saas.ViewModel.DevolucionCompra
{
    public class DetalleDevolucionCompraResumenVM
    {
        public int ProductoId { get; set; }

        public string ProductoNombre { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}