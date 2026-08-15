using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        public string Nombre { get; set; } = null!;
        public bool Estado { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
        [ValidateNever]
        public ICollection<MovimientoStock> MovimientosStock { get; set; }  = new List<MovimientoStock>();
        [ValidateNever]
        public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        [ValidateNever]
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        [ValidateNever]
        public ICollection<Caja> Cajas { get; set; } = new List<Caja>();

        [ValidateNever]
        public ICollection<MedioPago> MediosPago { get; set; } = new List<MedioPago>();

        [ValidateNever]
        public ICollection<CategoriaGasto> CategoriasGasto { get; set; } = new List<CategoriaGasto>();

        [ValidateNever]
        public ICollection<TurnoCaja> TurnosCaja { get; set; } = new List<TurnoCaja>();

        [ValidateNever]
        public ICollection<CobroVenta> CobrosVenta { get; set; } = new List<CobroVenta>();

        [ValidateNever]
        public ICollection<PagoProveedor> PagosProveedor { get; set; } = new List<PagoProveedor>();

        [ValidateNever]
        public ICollection<ReintegroVenta> ReintegrosVenta { get; set; } = new List<ReintegroVenta>();

        [ValidateNever]
        public ICollection<ReintegroProveedor> ReintegrosProveedor { get; set; } = new List<ReintegroProveedor>();

        [ValidateNever]
        public ICollection<TransferenciaCaja> TransferenciasCaja { get; set; } = new List<TransferenciaCaja>();

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; } = new List<MovimientoCaja>();
    }
}
