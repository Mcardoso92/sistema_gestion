namespace saas.ViewModel
{
    public class VentaDetailsVM
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public int EmpresaId { get; set; }

        public string EmpresaNombre { get; set; } = null!;

        public string UsuarioNombre { get; set; } = null!;

        public string ClienteNombre { get; set; } = "Cliente ocasional";

        public string? ClienteDocumento { get; set; }

        public string? ClienteEmail { get; set; }

        public List<VentaDetalleDetailsVM> Detalles { get; set; } = new();

        public int TotalLineas => Detalles.Count;

        public int TotalUnidades => Detalles.Sum(d => d.Cantidad);
    }
}