using saas.Models;

namespace saas.Services
{
    public interface IRevisionNotificacionService
    {
        Task<DateTime?> ObtenerUltimaRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin);

        Task<IReadOnlyDictionary<int, DateTime>> ObtenerUltimasRevisionesAsync(
            Usuario usuario,
            IEnumerable<int> empresaIds,
            bool esSuperAdmin);

        Task RegistrarRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin);

        Task RegistrarRevisionesAsync(
            Usuario usuario,
            IEnumerable<int> empresaIds,
            bool esSuperAdmin);
    }
}
