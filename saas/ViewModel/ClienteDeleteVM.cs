namespace saas.ViewModel
{
    public class ClienteDeleteVM
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Apellido { get; set; }

        public string? Documento { get; set; }

        public string? Email { get; set; }

        public string Empresa { get; set; } = null!;

        public bool Estado { get; set; }
    }
}
