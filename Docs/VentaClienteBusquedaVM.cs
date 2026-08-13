namespace saas.ViewModel
{
    public class VentaClienteBusquedaVM
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; } = null!;

        public string? Documento { get; set; }

        public string? Email { get; set; }

        public string? Telefono { get; set; }
    }
}