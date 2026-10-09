namespace saas.ViewModel
{
    public class PaginacionVM
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string NombreSingular { get; set; } = "registro encontrado";
        public string NombrePlural { get; set; } = "registros encontrados";

        // Cuando se informa un formulario, el paginador envía sus datos por POST.
        // Esto permite conservar selecciones y campos editados entre páginas.
        public string? FormularioId { get; set; }
        public string? AccionFormulario { get; set; }
        public string NombreCampoPagina { get; set; } = "pagina";
    }
}
