using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class CompraCreateVM
    {
        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        public int ProveedorId { get; set; }

        public int? EmpresaId { get; set; }

        [StringLength(30, ErrorMessage = "El tipo de comprobante no puede superar los 30 caracteres.")]
        public string? TipoComprobante { get; set; }

        [StringLength(50, ErrorMessage = "El número de comprobante no puede superar los 50 caracteres.")]
        public string? NumeroComprobante { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public List<DetalleCompraCreateVM> Detalles { get; set; } = new();

        public List<SelectListItem> Proveedores { get; set; } = new();

        public List<SelectListItem> Empresas { get; set; } = new();

        public List<SelectListItem> Productos { get; set; } = new();
    }
}