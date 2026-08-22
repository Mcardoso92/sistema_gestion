using System.ComponentModel.DataAnnotations;

namespace saas.ViewModels.DevolucionCompra
{
    public class RegistrarDevolucionCompraVM
    {
        public int CompraId { get; set; }

        public string ProveedorNombre { get; set; } = string.Empty;

        public DateTime FechaCompra { get; set; }

        public decimal TotalCompra { get; set; }

        [StringLength(
            500,
            ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public List<RegistrarDetalleDevolucionCompraVM> Detalles { get; set; }
            = new List<RegistrarDetalleDevolucionCompraVM>();
    }
}
