using saas.Models.Enums;

namespace saas.ViewModel
{
    public class StockHistorialItemVM
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string ProductoNombre { get; set; } = null!;
        public string? CodigoBarra { get; set; }
        public string EmpresaNombre { get; set; } = null!;
        public string UsuarioNombre { get; set; } = null!;
        public TipoMovimientoStock Tipo { get; set; }
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockPosterior { get; set; }
        public string? Motivo { get; set; }
        public int? VentaId { get; set; }
    }
}
