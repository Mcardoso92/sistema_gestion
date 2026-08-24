namespace saas.ViewModel.Dashboard
{
    public class DashboardVM
    {
        public decimal TotalVentasDia { get; set; }

        public int CantidadVentasDia { get; set; }

        public decimal TotalVentasMes { get; set; }

        public int CantidadVentasMes { get; set; }

        public List<ProductoStockBajoVM> ProductosStockBajo { get; set; } =  new List<ProductoStockBajoVM>();

        public List<ProductoMasVendidoVM> ProductosMasVendidos { get; set; } = new List<ProductoMasVendidoVM>();

        public List<ClienteFrecuenteVM> ClientesFrecuentes { get; set; } = new List<ClienteFrecuenteVM>();

        public List<VentaDiariaVM> VentasUltimosDias { get; set; } = new List<VentaDiariaVM>();
    }

    public class ProductoStockBajoVM
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? CodigoBarra { get; set; }

        public int Stock { get; set; }

        public int PuntoReposicion { get; set; }
    }

    public class ProductoMasVendidoVM
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int CantidadVendida { get; set; }

        public decimal ImporteVendido { get; set; }
    }

    public class ClienteFrecuenteVM
    {
        public int ClienteId { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;

        public int CantidadCompras { get; set; }

        public decimal ImporteComprado { get; set; }
    }

    public class VentaDiariaVM
    {
        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }
    }
}