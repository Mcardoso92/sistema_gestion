namespace saas.Services
{
    public record OrigenNotificacion(int EmpresaId, DateTime FechaOrigen);

    public class NotificacionNovedadService
    {
        public int CalcularCantidad(
            IEnumerable<OrigenNotificacion> avisos,
            IReadOnlyDictionary<int, DateTime> revisiones)
        {
            return avisos.Count(aviso =>
                !revisiones.TryGetValue(
                    aviso.EmpresaId,
                    out DateTime ultimaRevision) ||
                aviso.FechaOrigen > ultimaRevision);
        }
    }
}
