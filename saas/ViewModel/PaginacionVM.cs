namespace saas.ViewModel
{
    public class PaginacionVM
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string NombreSingular { get; set; } = "registro encontrado";
        public string NombrePlural { get; set; } = "registros encontrados";
    }
}
