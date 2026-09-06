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

        public string ClienteNombre { get; set; } = "Consumidor Final";

        public string? ClienteDocumento { get; set; }

        public string? ClienteEmail { get; set; }

        public List<VentaDetalleDetailsVM> Detalles { get; set; } = new();

        public int TotalLineas => Detalles.Count;

        public int TotalUnidades => Detalles.Sum(d => d.Cantidad);
        
        // Cobros
        public decimal TotalCobrado { get; set; }

        public decimal SaldoPendiente => Math.Max(0, Total - TotalCobrado);

        public bool TieneSaldoPendiente => SaldoPendiente > 0;

        public List<CobroVentaResumenVM> Cobros { get; set; }
            = new List<CobroVentaResumenVM>();

        public RegistrarCobroVentaVM NuevoCobro { get; set; }
            = new RegistrarCobroVentaVM();


        // Reintegros
        public decimal TotalReintegrado { get; set; }

        public decimal PendienteReintegrar { get; set; }

        public List<ReintegroVentaResumenVM> Reintegros { get; set; }
            = new List<ReintegroVentaResumenVM>();

        public RegistrarReintegroVentaVM NuevoReintegro { get; set; }
            = new RegistrarReintegroVentaVM();
    }
}
