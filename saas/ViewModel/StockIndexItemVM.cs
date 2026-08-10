using saas.ViewModel.Enums;

namespace saas.ViewModel
{
    public class StockIndexItemVM
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = null!;

        public string? CodigoBarra { get; set; }

        public string CategoriaNombre { get; set; } = null!;

        public string EmpresaNombre { get; set; } = null!;

        public int Stock { get; set; }

        public int PuntoReposicion { get; set; }

        public bool ProductoActivo { get; set; }

        public EstadoStockVM EstadoStock { get; set; }
    }
}