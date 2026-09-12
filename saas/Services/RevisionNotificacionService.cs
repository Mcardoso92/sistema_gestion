using Microsoft.EntityFrameworkCore;
using saas.Data;
using saas.Models;

namespace saas.Services
{
    public class RevisionNotificacionService : IRevisionNotificacionService
    {
        private readonly SaasDbContext _context;
        private readonly IFechaHoraService _fechaHora;

        public RevisionNotificacionService(
            SaasDbContext context,
            IFechaHoraService fechaHora)
        {
            _context = context;
            _fechaHora = fechaHora;
        }

        public async Task<DateTime?> ObtenerUltimaRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin)
        {
            await ValidarAccesoAsync(usuario, empresaId, esSuperAdmin);

            return await _context.RevisionesNotificacion
                .AsNoTracking()
                .Where(r =>
                    r.UsuarioId == usuario.Id &&
                    r.EmpresaId == empresaId)
                .Select(r => (DateTime?)r.FechaUltimaRevision)
                .SingleOrDefaultAsync();
        }

        public async Task<IReadOnlyDictionary<int, DateTime>> ObtenerUltimasRevisionesAsync(
            Usuario usuario,
            IEnumerable<int> empresaIds,
            bool esSuperAdmin)
        {
            int[] empresas = empresaIds.Distinct().ToArray();

            if (!esSuperAdmin && empresas.Any(id => id != usuario.EmpresaId))
            {
                throw new UnauthorizedAccessException(
                    "El usuario no puede acceder a las notificaciones de otra empresa.");
            }

            return await _context.RevisionesNotificacion
                .AsNoTracking()
                .Where(r =>
                    r.UsuarioId == usuario.Id &&
                    empresas.Contains(r.EmpresaId))
                .ToDictionaryAsync(
                    r => r.EmpresaId,
                    r => r.FechaUltimaRevision);
        }

        public async Task RegistrarRevisionAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin)
        {
            await RegistrarRevisionesAsync(
                usuario,
                new[] { empresaId },
                esSuperAdmin);
        }

        public async Task RegistrarRevisionesAsync(
            Usuario usuario,
            IEnumerable<int> empresaIds,
            bool esSuperAdmin)
        {
            int[] empresas = empresaIds.Distinct().ToArray();

            foreach (int empresaId in empresas)
            {
                await ValidarAccesoAsync(usuario, empresaId, esSuperAdmin);
            }

            Dictionary<int, RevisionNotificacion> revisiones =
                await _context.RevisionesNotificacion
                    .Where(r =>
                        r.UsuarioId == usuario.Id &&
                        empresas.Contains(r.EmpresaId))
                    .ToDictionaryAsync(r => r.EmpresaId);

            DateTime fechaRevision = _fechaHora.UtcAhora;

            foreach (int empresaId in empresas)
            {
                if (!revisiones.TryGetValue(
                    empresaId,
                    out RevisionNotificacion? revision))
                {
                    revision = new RevisionNotificacion
                    {
                        UsuarioId = usuario.Id,
                        EmpresaId = empresaId
                    };

                    _context.RevisionesNotificacion.Add(revision);
                }

                revision.FechaUltimaRevision = fechaRevision;
            }

            await _context.SaveChangesAsync();
        }

        private async Task ValidarAccesoAsync(
            Usuario usuario,
            int empresaId,
            bool esSuperAdmin)
        {
            if (!esSuperAdmin && usuario.EmpresaId != empresaId)
            {
                throw new UnauthorizedAccessException(
                    "El usuario no puede acceder a las notificaciones de otra empresa.");
            }

            bool empresaExiste = await _context.Empresas
                .AsNoTracking()
                .AnyAsync(e => e.Id == empresaId);

            if (!empresaExiste)
            {
                throw new InvalidOperationException(
                    "La empresa indicada no existe.");
            }
        }
    }
}
