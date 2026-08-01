namespace saas.ViewModel
{
    public class UsuarioDeleteVM
    {
        public string Id { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Empresa { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public bool Estado { get; set; }
    }
}
