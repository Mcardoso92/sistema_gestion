using saas.Models;

namespace saas.Services
{
    public interface IRevisionNotificacionService
    {
        Task<DateTime?> ObtenerUltimaRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin);

        Task RegistrarRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin);
    }
}
