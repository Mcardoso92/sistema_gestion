namespace saas.ViewModel.DevolucionCompra
{
    public class DevolucionCompraResumenVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public string UsuarioNombre { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public string? UsuarioAnulacionNombre { get; set; }

        public string? MotivoAnulacion { get; set; }

        public List<DetalleDevolucionCompraResumenVM> Detalles { get; set; }
            = new List<DetalleDevolucionCompraResumenVM>();
    }
}