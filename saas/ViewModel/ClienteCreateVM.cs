using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class ClienteCreateVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = null!;

        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string? Apellido { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
        public string? Documento { get; set; }

        [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
        [StringLength(256, ErrorMessage = "El email no puede superar los 256 caracteres.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Ingrese un teléfono válido.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [StringLength(150, ErrorMessage = "La dirección no puede superar los 150 caracteres.")]
        public string? Direccion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una empresa.")]
        public int EmpresaId { get; set; }

        public IEnumerable<SelectListItem> Empresas { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
