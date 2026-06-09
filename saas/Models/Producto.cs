using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Producto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        [StringLength(500)]
        public string Descripcion { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int PuntoReposicion { get; set; }
        public bool Estado { get; set; }
        [Url]
        public string UrlImagen { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<DetalleVenta>? DetallesVenta { get; set; }

        //rowversion para control de concurrencia optimista
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
