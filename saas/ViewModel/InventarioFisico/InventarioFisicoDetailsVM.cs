using saas.Models.Enums;

namespace saas.ViewModel.InventarioFisico
{
    public class InventarioFisicoDetailsVM
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public EstadoInventarioFisico Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaConfirmacion { get; set; }
        public string UsuarioInicio { get; set; } = string.Empty;
        public string? UsuarioConfirmacion { get; set; }
        public List<InventarioFisicoDetalleVM> Detalles { get; set; } = new();
        public bool EstaAbierto => Estado == EstadoInventarioFisico.Abierto;
        public int Diferencias => Detalles.Count(d => d.StockContado.HasValue && d.StockContado != d.StockTeorico);
        public bool ConteoCompleto => Detalles.Count > 0 && Detalles.All(d => d.StockContado.HasValue);
    }

    public class InventarioFisicoDetalleVM
    {
        public int DetalleId { get; set; }
        public int ProductoId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string? CodigoBarra { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int StockTeorico { get; set; }
        public int? StockContado { get; set; }
        public int? Diferencia => StockContado - StockTeorico;
    }
}
