namespace saas.ViewModel.ProductoImportacion
{
    public class ProductoImportacionVistaPreviaVM
    {
        // El token identifica el análisis temporal y evita reenviar todas las filas desde el navegador al confirmar.
        public string Token { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public List<ProductoImportacionFilaVM> Filas { get; set; } = new List<ProductoImportacionFilaVM>();
        public int TotalFilas => Filas.Count;
        public int FilasValidas => Filas.Count(f => f.EsValida);
        public int FilasConErrores => Filas.Count(f => !f.EsValida);
        public bool PuedeImportar => TotalFilas > 0 && FilasConErrores == 0;
    }
}
