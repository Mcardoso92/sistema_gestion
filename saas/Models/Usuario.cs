using Microsoft.AspNetCore.Identity;

namespace saas.Models
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaAlta { get; set; }

    }
}
