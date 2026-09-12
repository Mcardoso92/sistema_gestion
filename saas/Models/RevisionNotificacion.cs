using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace saas.Models
{
    public class RevisionNotificacion
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = null!;

        [ValidateNever]
        public Usuario Usuario { get; set; } = null!;

        public int EmpresaId { get; set; }

        [ValidateNever]
        public Empresa Empresa { get; set; } = null!;

        public DateTime FechaUltimaRevision { get; set; }
    }
}
