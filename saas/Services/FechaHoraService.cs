using Microsoft.Extensions.Options;
using saas.Settings;

namespace saas.Services
{
    public sealed class FechaHoraService : IFechaHoraService
    {
        private readonly TimeProvider _timeProvider;
        private readonly TimeZoneInfo _zonaHoraria;

        public FechaHoraService(
            TimeProvider timeProvider,
            IOptions<ZonaHorariaSettings> settings)
        {
            _timeProvider = timeProvider;
            _zonaHoraria = ResolverZonaHoraria(settings.Value.Id);
        }

        public DateTime UtcAhora => _timeProvider.GetUtcNow().UtcDateTime;

        public DateTime HoraLocalAhora => ConvertirAHoraLocal(UtcAhora);

        public DateTime FechaLocalHoy => HoraLocalAhora.Date;

        public DateTime ConvertirAHoraLocal(DateTime fechaUtc)
        {
            DateTime utc = DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, _zonaHoraria);
        }

        public DateTime? ConvertirAHoraLocal(DateTime? fechaUtc) =>
            fechaUtc.HasValue
                ? ConvertirAHoraLocal(fechaUtc.Value)
                : null;

        public DateTime ConvertirAUtc(DateTime fechaLocal)
        {
            DateTime local = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, _zonaHoraria);
        }

        private static TimeZoneInfo ResolverZonaHoraria(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("Debe configurarse una zona horaria para Veltika.");
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException) when (id == "America/Argentina/Buenos_Aires")
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
        }
    }
}
