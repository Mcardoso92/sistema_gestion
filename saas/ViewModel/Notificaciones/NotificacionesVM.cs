namespace saas.ViewModel.Notificaciones
{
    public class NotificacionesVM
    {
        public int CantidadStockBajo { get; set; }

        public int CantidadSinStock { get; set; }

        public int CantidadVentasImportantes { get; set; }

        public int CantidadTotal => CantidadStockBajo + CantidadSinStock + CantidadVentasImportantes;

        public List<NotificacionStockItemVM> Productos { get; set; } = new List<NotificacionStockItemVM>();
        public List<NotificacionVentaItemVM> Ventas { get; set; } = new List<NotificacionVentaItemVM>();
    }

    public class NotificacionStockItemVM
    {
        public int ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Empresa { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int PuntoReposicion { get; set; }

        public bool SinStock => Stock == 0;
    }
    public class NotificacionVentaItemVM
    {
        public int VentaId { get; set; }

        public DateTime Fecha { get; set; }

        public string Cliente { get; set; } = string.Empty;

        public string Empresa { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}