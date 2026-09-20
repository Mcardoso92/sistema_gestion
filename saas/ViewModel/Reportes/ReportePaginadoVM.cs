namespace saas.ViewModel.Reportes
{
    // Estado común del listado visible; las métricas de cada reporte no dependen de la página.
    public abstract class ReportePaginadoVM
    {
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }
    }
}
