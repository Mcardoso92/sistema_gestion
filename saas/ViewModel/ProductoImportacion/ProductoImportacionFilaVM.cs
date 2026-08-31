namespace saas.ViewModel.ProductoImportacion
{
    public class ProductoImportacionFilaVM
    {
        public int NumeroFila { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? CodigoBarra { get; set; }
        public string? Categoria { get; set; }
        public int? CategoriaId { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockInicial { get; set; }
        public int PuntoReposicion { get; set; }
        public string? Descripcion { get; set; }

        // Conserva todos los problemas de la fila para que el usuario pueda corregirlos en una sola revisión.
        public List<string> Errores { get; set; } = new List<string>();
        public bool EsValida => Errores.Count == 0;
    }
}
