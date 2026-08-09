namespace saas.ViewModel
{
    public class VentaDetalleDetailsVM
    {
        public int ProductoId { get; set; }

        public string ProductoNombre { get; set; } = null!;

        public string? CodigoBarra { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int Cantidad { get; set; }

        public decimal Subtotal { get; set; }
    }
}