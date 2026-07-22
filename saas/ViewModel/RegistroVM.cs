using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace saas.ViewModel
{
    public class RegistroVM
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public int EmpresaId { get; set; }
        [PasswordPropertyText]
        public string Clave { get; set; }
        [PasswordPropertyText]
        public string ConfirmarClave { get; set; }
        public IEnumerable<SelectListItem> Empresas { get; set; }
    }
}
