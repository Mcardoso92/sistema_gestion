using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace saas.Models
{
    public class MedioPago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaAlta { get; set; }

        public int EmpresaId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        [ValidateNever]
        public ICollection<CajaMedioPago> CajaMediosPago { get; set; }
            = new List<CajaMedioPago>();

        [ValidateNever]
        public ICollection<MovimientoCaja> MovimientosCaja { get; set; }
            = new List<MovimientoCaja>();

        [ValidateNever]
        public ICollection<CobroVenta> CobrosVenta { get; set; }
            = new List<CobroVenta>();

        [ValidateNever]
        public ICollection<PagoProveedor> PagosProveedor { get; set; }
            = new List<PagoProveedor>();

        [ValidateNever]
        public ICollection<ReintegroVenta> ReintegrosVenta { get; set; }
            = new List<ReintegroVenta>();

        [ValidateNever]
        public ICollection<ReintegroProveedor> ReintegrosProveedor { get; set; }
            = new List<ReintegroProveedor>();
    }
}