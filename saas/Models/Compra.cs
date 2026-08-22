using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Compra
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        [StringLength(30, ErrorMessage = "El tipo de comprobante no puede superar los 30 caracteres.")]
        public string? TipoComprobante { get; set; }

        [StringLength(50, ErrorMessage = "El número de comprobante no puede superar los 50 caracteres.")]
        public string? NumeroComprobante { get; set; }

        public decimal Total { get; set; }

        public bool Estado { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public DateTime? FechaAnulacion { get; set; }

        public int EmpresaId { get; set; }

        public int ProveedorId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = null!;

        public string? UsuarioAnulacionId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public Proveedor Proveedor { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        [ValidateNever]
        public Usuario? UsuarioAnulacion { get; set; }

        [ValidateNever]
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();

        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
        [ValidateNever]
        public ICollection<PagoProveedor> PagosProveedor { get; set; } = new List<PagoProveedor>();

        [ValidateNever]
        public ICollection<ReintegroProveedor> ReintegrosProveedor { get; set; } = new List<ReintegroProveedor>();
        [ValidateNever]
        public ICollection<DevolucionCompra> DevolucionesCompra { get; set; } = new List<DevolucionCompra>();
    }
}