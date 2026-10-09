namespace saas.ViewModel.ProductoActualizacionMasiva
{
    public class ProductoActualizacionMasivaFilaVM
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal CostoAnterior { get; set; }
        public decimal CostoNuevo { get; set; }
        public decimal VentaAnterior { get; set; }
        public decimal VentaNueva { get; set; }
        public bool ModificaCosto => CostoAnterior != CostoNuevo;
        public bool ModificaVenta => VentaAnterior != VentaNueva;
    }
}
