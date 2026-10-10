using saas.Data;

namespace saas.Services
{
    public class BienvenidaUsuarioService
    {
        private readonly SaasDbContext _context;

        public BienvenidaUsuarioService(SaasDbContext context)
        {
            _context = context;
        }

        public async Task MarcarComoVisualizadaAsync(string usuarioId)
        {
            // La actualización es idempotente y sólo alcanza al usuario autenticado
            // que el controller resolvió; no depende del navegador ni de la sesión.
            var usuario = await _context.Users.FindAsync(usuarioId);

            if (usuario == null || usuario.BienvenidaVisualizada)
            {
                return;
            }

            usuario.BienvenidaVisualizada = true;
            await _context.SaveChangesAsync();
        }
    }
}
