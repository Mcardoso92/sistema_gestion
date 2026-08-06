using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Venta
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public bool Estado { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public string? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<DetalleVenta>? Detalles { get; set; }

    }
}
