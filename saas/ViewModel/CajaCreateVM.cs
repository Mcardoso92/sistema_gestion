using Microsoft.AspNetCore.Mvc.Rendering;
using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class CajaCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        public TipoCaja Tipo { get; set; }

        public bool PermiteTurnos { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "El fondo fijo no puede ser negativo.")]
        public decimal FondoFijo { get; set; }

        public int? EmpresaId { get; set; }
        public List<SelectListItem> Empresas { get; set; }
            = new List<SelectListItem>();
        public List<int> MediosPagoSeleccionadosIds { get; set; }
            = new List<int>();

        public List<MedioPagoOpcionVM> MediosPagoDisponibles { get; set; }
            = new List<MedioPagoOpcionVM>();
        
    }
}