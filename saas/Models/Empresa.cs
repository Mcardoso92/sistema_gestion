namespace saas.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public ICollection<Usuario>? Usuarios { get; set; }
        public ICollection<Producto>? Productos { get; set; }
        public ICollection<Venta>? Ventas { get; set; }

    }
}
