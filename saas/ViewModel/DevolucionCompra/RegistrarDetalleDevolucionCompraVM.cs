using System.ComponentModel.DataAnnotations;

namespace saas.ViewModels.DevolucionCompra
{
    public class RegistrarDetalleDevolucionCompraVM
    {
        public int DetalleCompraId { get; set; }

        public int ProductoId { get; set; }

        public string ProductoNombre { get; set; } = string.Empty;

        public int CantidadComprada { get; set; }

        public int CantidadDevuelta { get; set; }

        public int CantidadDisponible { get; set; }

        public decimal PrecioUnitario { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "La cantidad a devolver no puede ser negativa.")]
        public int CantidadDevolver { get; set; }
    }
}