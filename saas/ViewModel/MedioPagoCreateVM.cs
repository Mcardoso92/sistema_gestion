using saas.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class MedioPagoCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = null!;

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string? Descripcion { get; set; }
        public TipoMedioPago Tipo { get; set; }

        public int? EmpresaId { get; set; }
    }
}