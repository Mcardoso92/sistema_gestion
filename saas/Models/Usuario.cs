using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Usuario : IdentityUser
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = null!;
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string Apellido { get; set; } = null!;
        [StringLength(500, ErrorMessage = "La URL de la imagen no puede superar los 500 caracteres.")]
        public string? ImagenPerfil { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = null!;
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        [ValidateNever]
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
        [ValidateNever]
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        [ValidateNever]
        public ICollection<Compra> ComprasAnuladas { get; set; } = new List<Compra>();
        [ValidateNever]
        public ICollection<TurnoCaja> TurnosCajaApertura { get; set; } = new List<TurnoCaja>();

        [ValidateNever]
        public ICollection<TurnoCaja> TurnosCajaCierre { get; set; } = new List<TurnoCaja>();

        [ValidateNever]
        public ICollection<CobroVenta> CobrosVenta { get; set; } = new List<CobroVenta>();

        [ValidateNever]
        public ICollection<CobroVenta> CobrosVentaAnulados { get; set; } = new List<CobroVenta>();

        [ValidateNever]
        public ICollection<PagoProveedor> PagosProveedor { get; set; } = new List<PagoProveedor>();

        [ValidateNever]
        public ICollection<PagoProveedor> PagosProveedorAnulados { get; set; } = new List<PagoProveedor>();

        [ValidateNever]
        public ICollection<ReintegroVenta> ReintegrosVenta { get; set; } = new List<ReintegroVenta>();

        [ValidateNever]
        public ICollection<ReintegroVenta> ReintegrosVentaAnulados { get; set; } = new List<ReintegroVenta>();

        [ValidateNever]
        public ICollection<ReintegroProveedor> ReintegrosProveedor { get; set; } = new List<ReintegroProveedor>();

        [ValidateNever]
        public ICollection<ReintegroProveedor> ReintegrosProveedorAnulados { get; set; } = new List<ReintegroProveedor>();

        [ValidateNever]
        public ICollection<TransferenciaCaja> TransferenciasCaja { get; set; } = new List<TransferenciaCaja>();

        [ValidateNever]
        public ICollection<TransferenciaCaja> TransferenciasCajaAnuladas { get; set; } = new List<TransferenciaCaja>();

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();
    }
}
