namespace saas.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int PuntoReposicion { get; set; }
        public bool Estado { get; set; }
        public string UrlImagen { get; set; }
        public DateTime FechaAlta { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }
    }
}
