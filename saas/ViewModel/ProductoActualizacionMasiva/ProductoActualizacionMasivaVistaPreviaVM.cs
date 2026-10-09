using saas.Models.Enums;

namespace saas.ViewModel.ProductoActualizacionMasiva
{
    public class ProductoActualizacionMasivaVistaPreviaVM
    {
        public string Token { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public int? CategoriaId { get; set; }
        public string? Busqueda { get; set; }
        public CampoAjustePrecioProducto Campo { get; set; }
        public TipoAjustePrecioProducto TipoAjuste { get; set; }
        public decimal ValorAjuste { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public List<ProductoActualizacionMasivaFilaVM> Productos { get; set; } = new();
        public int TotalProductos => Productos.Count;
    }
}
